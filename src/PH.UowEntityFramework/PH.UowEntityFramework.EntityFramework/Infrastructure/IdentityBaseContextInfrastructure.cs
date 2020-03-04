using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Proxies.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PH.UowEntityFramework.EntityFramework.Abstractions.Models;
using PH.UowEntityFramework.EntityFramework.Audit;
using PH.UowEntityFramework.EntityFramework.Extensions;
using PH.UowEntityFramework.EntityFramework.Mapping;
using PH.UowEntityFramework.UnitOfWork;

namespace PH.UowEntityFramework.EntityFramework.Infrastructure
{
    /// <summary>
    /// Identitity Context Infrastructure
    /// </summary>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    /// <typeparam name="TRole">The type of the role.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext{TUser, TRole, TKey}" />
    public abstract class IdentityBaseContextInfrastructure<TUser, TRole, TKey> :
        Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<TUser, TRole, TKey>
        , IIdentityBaseContext<TUser, TRole, TKey>, IAuditContext
        where TUser : IdentityUser<TKey>, IEntity<TKey>
        where TRole : IdentityRole<TKey>, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>Gets or sets the author.</summary>
        /// <value>The author.</value>
        public string Author { get; set; }

        /// <summary>
        /// Ctx Uid
        /// </summary>
        public Guid CtxUid { get; internal set; }


        /// <summary>
        /// Transaction Audit
        /// </summary>
        public DbSet<TransactionAudit> TransactionAudits { get; set; }

        ///// <summary>
        ///// Tenant
        ///// </summary>
        //public DbSet<Tenant> Tenants { get; set; }


        /// <summary>Gets or sets the audits.</summary>
        /// <value>The audits.</value>
        internal DbSet<Audit.Audit> Audits { get; set; }

        /// <summary>
        /// Identifier
        /// </summary>
        public string Identifier { get; set; }


        /// <summary>Gets or sets the logger.</summary>
        /// <value>The logger.</value>
        public ILogger Logger { get; set; }

        private TransactionAudit _currenTransactionAudit;

        

        /// <summary>
        /// Gets or sets a value indicating whether this  is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        public bool Initialized { get; protected set; }


        /// <summary>Gets the scope dictionary.</summary>
        /// <value>The scope dictionary.</value>
        public Dictionary<int, string> ScopeDictionary { get; }

        private int _scopeCount;


        /// <summary>The transaction</summary>
        protected IDbContextTransaction Transaction;

        /// <summary>Gets or sets the cancellation token source.</summary>
        /// <value>The cancellation token source.</value>
        public CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>Gets or sets the cancellation token.</summary>
        /// <value>The cancellation token.</value>
        public CancellationToken CancellationToken { get; protected set; }

        /// <summary>Gets the entity json serializer options.</summary>
        /// <value>The entity json serializer options.</value>
        public JsonSerializerOptions EntityJsonSerializerOptions { get; }

        /// <summary>
        /// FOR MIGRATION Initializes a new instance of the <see cref="IdentityBaseContextInfrastructure{TUser, TRole, TKey}"/> class.
        /// </summary>
        /// <param name="migrationTime">The migration time.</param>
        /// <param name="options">The options.</param>
        protected IdentityBaseContextInfrastructure(DateTime migrationTime, DbContextOptions options)
            : this(options, string.Empty, string.Empty, false, false)
        {
        }

        private IdentityBaseContextInfrastructure([NotNull] DbContextOptions options, [CanBeNull] string identifier,
                                                  string author, bool auditingEnabled, bool initTransaction = true)
            : base(options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            AuditingEnabled         = auditingEnabled;
            Changecount             = 0;
            Initialized             = false;
            CtxUid                  = NewId.NextGuid();
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken       = CancellationTokenSource.Token;
            ScopeDictionary         = new Dictionary<int, string>();
            _scopeCount             = 0;


            Identifier = string.IsNullOrEmpty(identifier) ? $"{CtxUid:N}" : identifier;
            Author     = author;

            EntityJsonSerializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = false,
                Converters    = {new EntityToJsonConverterFactory(), new DateTimeConverter()}
            };

            var auditTran = EnsureTransactionAuditAsync().GetAwaiter().GetResult();
            if (initTransaction)
            {
                BeginTransaction();
            }

            ChangeTracker.StateChanged += async (sender, args) => await TrackAuditableEntityAsync(args);
            ChangeTracker.Tracked += async (sender, args) => await TrackAuditableEntityAsync(args);
            
        }

        /// <summary>Tracks the auditable entity asynchronous.</summary>
        /// <param name="argument">The <see cref="EntityStateChangedEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        protected async Task<bool> TrackAuditableEntityAsync(EntityStateChangedEventArgs argument)
        {
            bool b = false;
            switch (argument.NewState)
            {
                case EntityState.Detached:
                case EntityState.Unchanged:
                    break;
                case EntityState.Deleted:
                case EntityState.Modified:
                case EntityState.Added:
                    b = await TrackStatePrivateAsync(argument.Entry, argument.Entry.State);
                    break;
               
            }
            return b;
        }

        /// <summary>Tracks the auditable entity asynchronous.</summary>
        /// <param name="argument">The <see cref="EntityTrackedEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        protected async Task<bool> TrackAuditableEntityAsync([NotNull] EntityTrackedEventArgs argument)
        {
            bool b = false;
            switch (argument.Entry.State)
            {
                case EntityState.Detached:
                case EntityState.Unchanged:
                    break;

                case EntityState.Deleted:
                case EntityState.Modified:
                case EntityState.Added:
                    b = await TrackStatePrivateAsync(argument.Entry, argument.Entry.State);
                    break;
            }

            return b;
        }

        private async Task<bool> TrackStatePrivateAsync([NotNull] EntityEntry entry, EntityState state)
        {
            if (entry.Entity is IEntity e)
            {
                Changecount++;

                var auditTran = await EnsureTransactionAuditAsync();
                EventMethod method = EventMethod.Add;
                switch (state)
                {
                    case EntityState.Deleted:
                        break;
                    case EntityState.Modified:

                        e.UpdatedTransaction = auditTran;
                        e.UpdatedTransactionId = auditTran.Id;
                        method = EventMethod.Update;

                        if (null != e.DeletedTransaction)
                        {
                            method = EventMethod.Delete;
                        }

                        break;
                    case EntityState.Added:
                        e.CreatedTransaction = auditTran;
                        e.CreatedTransactionId = auditTran.Id;
                        e.UpdatedTransaction   = auditTran;
                        e.UpdatedTransactionId = auditTran.Id;
                        break;
                }

                if (AuditingEnabled)
                {
                        
                    var t = FindEntityType(e);

                    
                    //var json = System.Text.Json.JsonSerializer.Serialize(entity, EntityJsonSerializerOptions);
                    var vl = JsonSerializer.SerializeToUtf8Bytes(entry.Entity, EntityJsonSerializerOptions);
                    var a = new Audit.Audit()
                    {
                        Author        = Author,
                        Action        = $"{method}",
                        DateTime      = DateTime.UtcNow,
                        EntityName    = t.entityType.ClrType.Name,
                        TableName     = t.TableName,
                        Id            = $"{NewId.NextGuid():N}",
                        KeyValue      = $"{GetIdValue(entry.Entity)}",
                        TransactionId = auditTran.Id,
                        Values        = vl
                    };
                    await Audits.AddAsync(a);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityBaseContextInfrastructure{TUser, TRole, TKey}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="author">The author.</param>
        /// <param name="auditingEnabled">if audits enabled</param>
        /// <exception cref="ArgumentNullException">options</exception>
        protected IdentityBaseContextInfrastructure([NotNull] DbContextOptions options, [CanBeNull] string identifier,
                                                    string author, bool auditingEnabled)
            : this(options, identifier, author, auditingEnabled, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityBaseContextInfrastructure{TUser, TRole, TKey}"/> class.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="T:Microsoft.EntityFrameworkCore.DbContext" />.</param>
        /// <param name="auditingEnabled">if auditing enabled</param>
        protected IdentityBaseContextInfrastructure([NotNull] DbContextOptions options, bool auditingEnabled = false)
            : this(options, string.Empty, string.Empty, auditingEnabled, true)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether auditing enabled.
        /// </summary>
        /// <value><c>true</c> if auditing enabled; otherwise, <c>false</c>.</value>
        public bool AuditingEnabled { get; set; }

        /// <summary>Gets or sets the changecount.</summary>
        /// <value>The changecount.</value>
        public int Changecount { get; protected set; }


        #region Db Contenxt and Config

        /// <summary>
        /// Configures the schema needed for the identity framework. Do not use: use instead <see cref="OnCustomModelCreating"/>
        /// </summary>
        /// <param name="builder">
        /// The builder being used to construct the model for this context.
        /// </param>
        protected sealed override void OnModelCreating([NotNull] ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            OnCustomModelCreating(builder);

            builder.ApplyConfiguration(new TransactionAuditMap());
            AssignTenantAndOtherQueryFilterOnModelCreating(builder);
        }

        /// <summary>
        /// Custom On Model Creating
        /// </summary>
        /// <param name="builder">The builder being used to construct the model for this context.</param>
        public abstract void OnCustomModelCreating(ModelBuilder builder);


        /// <summary>
        /// Assign Query Filters base and other set on <see cref="IEntityTypeConfiguration{TEntity}">map</see>
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void AssignTenantAndOtherQueryFilterOnModelCreating([NotNull] ModelBuilder builder)
        {

            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            var allTypes = ScanAssemblyTypes();
            var tt       = GetAllEntityTypes(allTypes);


            foreach (Type entityType in tt)
            {
                var entityName     = entityType.Name;
                var entityFullName = entityType.FullName ?? entityName;

                try
                {
                    #region QueryFilters

                    var paramName = $"{entityName}_p";

                    var entityTypeFromModel = builder.Model.FindEntityType(entityFullName);
                    var queryFilter         = entityTypeFromModel.GetQueryFilter();
                    var queryParam          = queryFilter?.Parameters.FirstOrDefault();
                    if (null != queryParam)
                    {
                        paramName = queryParam.Name;
                    }


                    ParameterExpression paramExpr = Expression.Parameter(entityType, paramName);


                    Expression bodyDeleted = Expression.Equal(Expression.Property(paramExpr, "Deleted"),
                                                              Expression.Constant(false)
                                                             );
                    Expression body = null;
                    if (null != queryFilter)
                    {
                        var res = Expression.Lambda(Expression.Invoke(queryFilter, paramExpr), paramExpr);


                        body = Expression.AndAlso(bodyDeleted, res.Body);
                    }
                    else
                    {
                        body = bodyDeleted;
                    }


                    var name = $"TenantQueryFilter_{entityName}";

                    LambdaExpression lambdaExpr = Expression.Lambda(body, name,
                                                                    new List<ParameterExpression>() {paramExpr}
                                                                   );

                    var entyTYpeBuilder = builder.Entity(entityType);


                    entyTYpeBuilder.HasQueryFilter(lambdaExpr);

                    #endregion
                }
                catch (Exception e)
                {
                    var err = $"Error configuring '{entityName}'";
                    ContextLogger?.LogCritical(err, e);
                    throw new Exception(err, e);
                }
            }
        }


        /// <summary>Scans the assembly types.</summary>
        /// <returns></returns>
        protected abstract Type[] ScanAssemblyTypes();

        /// <summary>Gets all entity types.</summary>
        /// <param name="allTypes">All types.</param>
        /// <returns></returns>
        [NotNull]
        protected virtual Type[] GetAllEntityTypes([NotNull] Type[] allTypes)
        {
            var entityTypes = allTypes.Where(x => x.IsClass && !x.IsAbstract && typeof(IEntity).IsAssignableFrom(x))
                                      .OrderBy(x => x.Name).ToArray();

            return entityTypes;
        }

        #endregion


        #region overriding members for ADD, Update, Delete ...

        #region TransactionAudit and Values modifiers

        /// <summary>Ensures the transaction audit.</summary>
        /// <returns></returns>
        protected virtual TransactionAudit EnsureTransactionAudit()
        {
            return EnsureTransactionAuditAsync().GetAwaiter().GetResult();
        }

        /// <summary>Ensures the transaction audit asynchronous.</summary>
        /// <returns></returns>
        protected virtual async Task<TransactionAudit> EnsureTransactionAuditAsync()
        {
            if (null == _currenTransactionAudit)
            {
                var t = new TransactionAudit()
                {
                    Author        = Author,
                    UtcDateTime   = DateTime.UtcNow,
                    StrIdentifier = Identifier
                };
                var e = await TransactionAudits.AddAsync(t);
                await SaveChangesAsync(true, CancellationToken);
                _currenTransactionAudit = e.Entity;
            }

            return _currenTransactionAudit;
        }

        



        private (IEntityType entityType, string TableName) FindEntityType(IEntity incoming)
        {
            Type t = null;
            if (incoming is IProxyLazyLoader proxy)
            {
                t = incoming.GetType().BaseType;
            }
            else
            {
                t = incoming.GetType();
            }

            var entityType = this.Model.FindEntityType(t);
            //var schema     = entityType?.GetSchema() ?? "";
            var tableName = entityType?.GetTableName() ?? "";

            return (entityType, tableName);
        }



        [CanBeNull]
        private static object GetIdValue([CanBeNull] object src)
        {
            return src?.GetType()?.GetProperty("Id")?.GetValue(src, null);
        }

        /// <summary>
        /// 
        /// </summary>
        protected enum EventMethod
        {
            /// <summary>The add operation</summary>
            Add,

            /// <summary>The update operation</summary>
            Update,

            /// <summary>The delete operation</summary>
            Delete
        }

        
        
        #endregion


        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state such that
        ///         they will be inserted into the database when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to add. </param>
        /// <returns>
        ///     The <see cref="T:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry`1" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public sealed override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        {
            if (!(entity is Audit.Audit) && !(entity is TransactionAudit))
            {
                var auditTran = EnsureTransactionAuditAsync().GetAwaiter().GetResult();
            }
            
            return base.Add(entity);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entity"> The entity to add. </param>
        /// <returns>
        ///     The <see cref="T:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public override EntityEntry Add(object entity)
        {
            if (!(entity is Audit.Audit) && !(entity is TransactionAudit))
            {
                var auditTran = EnsureTransactionAuditAsync().GetAwaiter().GetResult();
            }

            return base.Add(entity);
        }


        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        ///     <para>
        ///         This method is async only to allow special value generators, such as the one used by
        ///         'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo',
        ///         to access the database asynchronously. For all other cases the non async method should be used.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to add. </param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous Add operation. The task result contains the
        ///     <see cref="T:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry`1" /> for the entity. The entry provides access to change tracking
        ///     information and operations for the entity.
        /// </returns>
        public sealed override async ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(
            TEntity entity, CancellationToken cancellationToken = new CancellationToken())
        {
            if (!(entity is Audit.Audit) && !(entity is TransactionAudit))
            {
                var auditTran = await EnsureTransactionAuditAsync();
            }
            
            return await base.AddAsync(entity, cancellationToken);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        ///     <para>
        ///         This method is async only to allow special value generators, such as the one used by
        ///         'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo',
        ///         to access the database asynchronously. For all other cases the non async method should be used.
        ///     </para>
        /// </summary>
        /// <param name="entity"> The entity to add. </param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous Add operation. The task result contains the
        ///     <see cref="T:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry" /> for the entity. The entry provides access to change tracking
        ///     information and operations for the entity.
        /// </returns>
        public sealed override async ValueTask<EntityEntry> AddAsync(object entity,
                                                                     CancellationToken cancellationToken =
                                                                         new CancellationToken())
        {
            if (!(entity is Audit.Audit) && !(entity is TransactionAudit))
            {
                var auditTran = await EnsureTransactionAuditAsync();
            }
            
            return await base.AddAsync(entity, cancellationToken);
        }


        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity and entries reachable from the given entity using
        ///         the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Modified" /> state by default, but see below for cases
        ///         when a different state will be used.
        ///     </para>
        ///     <para>
        ///         Generally, no database interaction will be performed until <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. All entities found will be tracked
        ///         by the context.
        ///     </para>
        ///     <para>
        ///         For entity types with generated keys if an entity has its primary key value set
        ///         then it will be tracked in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Modified" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state.
        ///         This helps ensure new entities will be inserted, while existing entities will be updated.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         For entity types without generated keys, the state set is always <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Modified" />.
        ///     </para>
        ///     <para>
        ///         Use <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to update. </param>
        /// <returns>
        ///     The <see cref="T:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry`1" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public sealed override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        {
            if (!(entity is Audit.Audit) && !(entity is TransactionAudit))
            {
                var auditTran = EnsureTransactionAuditAsync().GetAwaiter().GetResult();
            }

            return base.Update(entity);
        }


        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity and entries reachable from the given entity using
        ///         the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Modified" /> state by default, but see below for cases
        ///         when a different state will be used.
        ///     </para>
        ///     <para>
        ///         Generally, no database interaction will be performed until <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. All entities found will be tracked
        ///         by the context.
        ///     </para>
        ///     <para>
        ///         For entity types with generated keys if an entity has its primary key value set
        ///         then it will be tracked in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Modified" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state.
        ///         This helps ensure new entities will be inserted, while existing entities will be updated.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         For entity types without generated keys, the state set is always <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Modified" />.
        ///     </para>
        ///     <para>
        ///         Use <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entity"> The entity to update. </param>
        /// <returns>
        ///     The <see cref="T:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public sealed override EntityEntry Update(object entity)
        {
            if (!(entity is Audit.Audit) && !(entity is TransactionAudit))
            {
                var auditTran = EnsureTransactionAuditAsync().GetAwaiter().GetResult();
            }

            return base.Update(entity);
        }


        /// <summary>
        ///     Begins tracking the given entity in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Deleted" /> state such that it will
        ///     be removed from the database when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If the entity is already tracked in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state then the context will
        ///         stop tracking the entity (rather than marking it as <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Deleted" />) since the
        ///         entity was previously added to the context and does not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="M:Microsoft.EntityFrameworkCore.DbContext.Attach``1(``0)" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </remarks>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <param name="entity"> The entity to remove. </param>
        /// <returns>
        ///     The <see cref="T:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry`1" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public sealed override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
        {
            if (entity is IEntity e)
            {
                var auditTran = EnsureTransactionAuditAsync().GetAwaiter().GetResult();
                e.DeletedTransactionId = auditTran.Id;
                e.DeletedTransaction = auditTran;
                e.Deleted = true;

                return base.Update(entity);

            }
            else
            {
                return base.Remove(entity);
            }
        }


        /// <summary>
        ///     Begins tracking the given entity in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Deleted" /> state such that it will
        ///     be removed from the database when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If the entity is already tracked in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state then the context will
        ///         stop tracking the entity (rather than marking it as <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Deleted" />) since the
        ///         entity was previously added to the context and does not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="M:Microsoft.EntityFrameworkCore.DbContext.Attach(System.Object)" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        ///     <para>
        ///         Use <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </remarks>
        /// <param name="entity"> The entity to remove. </param>
        /// <returns>
        ///     The <see cref="T:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry" /> for the entity. The entry provides
        ///     access to change tracking information and operations for the entity.
        /// </returns>
        public sealed override EntityEntry Remove(object entity)
        {
            if (entity is IEntity e)
            {
                var auditTran = EnsureTransactionAuditAsync().GetAwaiter().GetResult();
                e.DeletedTransactionId = auditTran.Id;
                e.DeletedTransaction   = auditTran;
                e.Deleted = true;

                return base.Update(entity);

                //var e = EnsureTransactionValue(entity, EventMethod.Delete);
                //return base.Update(e);
            }
            else
            {
                return base.Remove(entity);
            }
        }

        /// <summary>
        ///     Begins tracking the given entities, and any other reachable entities that are
        ///     not already being tracked, in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state such that they will
        ///     be inserted into the database when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        public sealed override void AddRange(params object[] entities)
        {

            var auditTran = EnsureTransactionAuditAsync().GetAwaiter().GetResult();
            base.AddRange(entities);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        ///     <para>
        ///         This method is async only to allow special value generators, such as the one used by
        ///         'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo',
        ///         to access the database asynchronously. For all other cases the non async method should be used.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public sealed override async Task AddRangeAsync(params object[] entities)
        {
            
                var auditTran = await EnsureTransactionAuditAsync();
           

            await base.AddRangeAsync(entities);
        }

        /// <summary>
        ///     Begins tracking the given entities, and any other reachable entities that are
        ///     not already being tracked, in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state such that they will
        ///     be inserted into the database when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        public sealed override void AddRange(IEnumerable<object> entities)
        {
            var auditTran = EnsureTransactionAuditAsync().GetAwaiter().GetResult();
           

            base.AddRange(entities);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entity, and any other reachable entities that are
        ///         not already being tracked, in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state such that they will
        ///         be inserted into the database when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        ///     <para>
        ///         This method is async only to allow special value generators, such as the one used by
        ///         'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo',
        ///         to access the database asynchronously. For all other cases the non async method should be used.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to add. </param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        public sealed override async Task AddRangeAsync(IEnumerable<object> entities,
                                                        CancellationToken cancellationToken = new CancellationToken())
        {
            var e = await EnsureTransactionAuditAsync();
            await base.AddRangeAsync(entities, cancellationToken);
        }


        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities and entries reachable from the given entities using
        ///         the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Modified" /> state by default, but see below for cases
        ///         when a different state will be used.
        ///     </para>
        ///     <para>
        ///         Generally, no database interaction will be performed until <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. All entities found will be tracked
        ///         by the context.
        ///     </para>
        ///     <para>
        ///         For entity types with generated keys if an entity has its primary key value set
        ///         then it will be tracked in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Modified" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state.
        ///         This helps ensure new entities will be inserted, while existing entities will be updated.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         For entity types without generated keys, the state set is always <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Modified" />.
        ///     </para>
        ///     <para>
        ///         Use <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to update. </param>
        public sealed override void UpdateRange(params object[] entities)
        {
            var e = EnsureTransactionAuditAsync().GetAwaiter().GetResult();
            base.UpdateRange(entities);
        }

        /// <summary>
        ///     <para>
        ///         Begins tracking the given entities and entries reachable from the given entities using
        ///         the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Modified" /> state by default, but see below for cases
        ///         when a different state will be used.
        ///     </para>
        ///     <para>
        ///         Generally, no database interaction will be performed until <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        ///     <para>
        ///         A recursive search of the navigation properties will be performed to find reachable entities
        ///         that are not already being tracked by the context. All entities found will be tracked
        ///         by the context.
        ///     </para>
        ///     <para>
        ///         For entity types with generated keys if an entity has its primary key value set
        ///         then it will be tracked in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Modified" /> state. If the primary key
        ///         value is not set then it will be tracked in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state.
        ///         This helps ensure new entities will be inserted, while existing entities will be updated.
        ///         An entity is considered to have its primary key value set if the primary key property is set
        ///         to anything other than the CLR default for the property type.
        ///     </para>
        ///     <para>
        ///         For entity types without generated keys, the state set is always <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Modified" />.
        ///     </para>
        ///     <para>
        ///         Use <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry.State" /> to set the state of only a single entity.
        ///     </para>
        /// </summary>
        /// <param name="entities"> The entities to update. </param>
        public sealed override void UpdateRange(IEnumerable<object> entities)
        {
            var e = EnsureTransactionAuditAsync().GetAwaiter().GetResult();
            base.UpdateRange(entities);
            
        }


        /// <summary>
        ///     Begins tracking the given entity in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Deleted" /> state such that it will
        ///     be removed from the database when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If any of the entities are already tracked in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state then the context will
        ///         stop tracking those entities (rather than marking them as <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Deleted" />) since those
        ///         entities were previously added to the context and do not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="M:Microsoft.EntityFrameworkCore.DbContext.AttachRange(System.Object[])" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        /// </remarks>
        /// <param name="entities"> The entities to remove. </param>
        public sealed override void RemoveRange(params object[] entities)
        {
            foreach (var entity in entities)
            {
                this.Remove(entity);
            }

        }


        /// <summary>
        ///     Begins tracking the given entity in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Deleted" /> state such that it will
        ///     be removed from the database when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If any of the entities are already tracked in the <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Added" /> state then the context will
        ///         stop tracking those entities (rather than marking them as <see cref="F:Microsoft.EntityFrameworkCore.EntityState.Deleted" />) since those
        ///         entities were previously added to the context and do not exist in the database.
        ///     </para>
        ///     <para>
        ///         Any other reachable entities that are not already being tracked will be tracked in the same way that
        ///         they would be if <see cref="M:Microsoft.EntityFrameworkCore.DbContext.AttachRange(System.Collections.Generic.IEnumerable{System.Object})" /> was called before calling this method.
        ///         This allows any cascading actions to be applied when <see cref="M:Microsoft.EntityFrameworkCore.DbContext.SaveChanges" /> is called.
        ///     </para>
        /// </remarks>
        /// <param name="entities"> The entities to remove. </param>
        public sealed override void RemoveRange(IEnumerable<object> entities)
        {
            this.RemoveRange(entities.ToArray());
        }

        #endregion


        #region Unit Of Work

        /// <summary>Begins the transaction.</summary>
        public Guid BeginTransaction()
        {
            return BeginTransactionAsync().GetAwaiter().GetResult();
        }

        /// <summary>Begins the transaction asynchronous.</summary>
        /// <returns></returns>
        public async Task<Guid> BeginTransactionAsync()
        {
            DisposeTransaction();
            Transaction = await Database.BeginTransactionAsync(CancellationToken);

            UowLogger?.LogDebug($"Initialized Uow with {nameof(Identifier)} '{Identifier}'");

            return Transaction.TransactionId;
        }


        /// <summary>Disposes the transaction.</summary>
        public void DisposeTransaction()
        {
            Transaction?.Dispose();
        }

        /// <summary>
        /// Perform Transaction Commit on a Db.
        /// On Error automatically perform a <see cref="IUnitOfWork.Rollback"/>
        /// </summary>
        public void Commit()
        {
            Commit("");
        }

        /// <summary>
        /// Perform Transaction Commit on a Db and write a custom log message related to this commit.
        /// On Error automatically perform a <see cref="IUnitOfWork.Rollback"/>
        /// </summary>
        /// <param name="logMessage"></param>
        public void Commit([CanBeNull] string logMessage)
        {
            CommitAsync(logMessage).GetAwaiter().GetResult();
        }

        /// <summary> Perform Transaction Commit on a Db and write a custom log message related to this commit asynchronous.
        ///
        /// On Error automatically perform a <see cref="Rollback"/>
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <returns></returns>
        public async Task<int> CommitAsync([CanBeNull] string logMessage)
        {
            int r = 0;

            var transactionCommitMessage = "";
            if (!string.IsNullOrEmpty(logMessage) && !string.IsNullOrWhiteSpace(logMessage))
            {
                UowLogger?.LogInformation($"Commit - {logMessage}");

                transactionCommitMessage = logMessage.Trim();
                if (transactionCommitMessage.Length > 500)
                {
                    transactionCommitMessage = transactionCommitMessage.Substring(0, 499);
                }
            }

            if (Changecount == 0)
            {
                UowLogger?.LogTrace("No changes to commit");
                Committed?.Invoke(this, UnitOfWorkEventArg.Instance(Identifier));
            }
            else
            {
                var d = DateTime.UtcNow - _currenTransactionAudit.UtcDateTime;
                _currenTransactionAudit.MillisecDuration = d.TotalMilliseconds;
                _currenTransactionAudit.StrIdentifier    = Identifier;


                if (transactionCommitMessage != "")
                {
                    _currenTransactionAudit.CommitMessage = transactionCommitMessage;
                }

                if (ScopeDictionary.Count > 0)
                {
                    var s = string.Join(" => "
                                        , ScopeDictionary.OrderBy(x => x.Key).Select(x => x.Value).ToArray());
                    if (s.Length > 500)
                    {
                        s = $"{s.Substring(0, 497)}...";
                    }

                    _currenTransactionAudit.Scopes = s;
                }


                TransactionAudits.Update(_currenTransactionAudit);
                r = await SaveChangesAsync(CancellationToken);


                UowLogger?.LogDebug($"Commit Transaction '{Identifier}'");
                try
                {
                    Transaction.Commit();
                    Committed?.Invoke(this, UnitOfWorkEventArg.Instance(Identifier));
                }
                catch (Exception e)
                {
                    UowLogger?.LogCritical(e, $"Error committing Transaction '{Identifier}': {e.Message}");
                    Rollback();
                    UowLogger?.LogInformation("Re-throw exception");
                    throw;
                }
            }

            return r;
        }

        /// <summary>
        /// Rollback changes on Db Transaction.
        ///
        /// <exception cref="Exception">On rollback error re-trow exception</exception>
        /// </summary>
        public void Rollback()
        {
            try
            {
                Transaction.Rollback();
                UowLogger?.LogTrace("Rollback");
            }
            catch (Exception e)
            {
                UowLogger?.LogCritical($"Error on Rollback: {e.Message}", e);
                throw;
            }
        }

        /// <summary>
        /// Fired On Committed Unit Of Work
        /// </summary>
        public event EventHandler<UnitOfWorkEventArg> Committed;

        /// <summary>Begins the scope.</summary>
        /// <param name="scopeName">Name of the scope.</param>
        /// <returns></returns>
        [NotNull]
        public IDisposable BeginScope(string scopeName)
        {
            if (string.IsNullOrEmpty(scopeName) || string.IsNullOrWhiteSpace(scopeName))
            {
                throw new ArgumentException("message", nameof(scopeName));
            }

            _scopeCount++;
            ScopeDictionary.Add(_scopeCount, scopeName);
            return new NamedScope(ContextLogger, scopeName);
        }

        #endregion

        /// <summary>Gets or sets the uow logger.</summary>
        /// <value>The uow logger.</value>
        public ILogger UowLogger { get; set; }

        /// <summary>Gets the context logger.</summary>
        /// <value>The context logger.</value>
        protected ILogger ContextLogger => Logger ?? UowLogger;
    }
}