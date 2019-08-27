using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PH.UowEntityFramework.EntityFramework.Abstractions.Models;
using PH.UowEntityFramework.EntityFramework.Audit;
using PH.UowEntityFramework.EntityFramework.Mapping;
using PH.UowEntityFramework.UnitOfWork;

namespace PH.UowEntityFramework.EntityFramework.Infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    /// <typeparam name="TRole">The type of the role.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext{TUser, TRole, TKey}" />
    public abstract class IdentityBaseContextInfrastructure<TUser, TRole, TKey> : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<TUser, TRole, TKey>, IIdentityBaseContext<TUser, TRole, TKey> , IAuditContext
        
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

        /// <summary>
        /// Current Transaction Audit.
        /// </summary>
        public TransactionAudit TransactionAudit { get; set; }

        

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


        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityBaseContextInfrastructure{TUser, TRole, TKey}"/> class.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="T:Microsoft.EntityFrameworkCore.DbContext" />.</param>
        protected IdentityBaseContextInfrastructure([NotNull] DbContextOptions options)
            :base(options)
        {
           
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Changecount = 0;
            Initialized = false;
            CtxUid                  = NewId.NextGuid(); 
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken       = CancellationTokenSource.Token;
            ScopeDictionary         = new Dictionary<int, string>();
            _scopeCount             = 0;
        }



        /// <summary>Gets or sets the changecount.</summary>
        /// <value>The changecount.</value>
        public int Changecount { get; protected set; }

        /// <summary>Saches the changes internal.</summary>
        /// <returns></returns>
        protected internal int SacheChangesInternal()
        {
            return base.SaveChanges();

        }
        /// <summary>Saves the changes internal asynchronous.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected internal async Task<int> SaveChangesInternalAsync(CancellationToken cancellationToken =
                                                                        new CancellationToken() )
        {
            return await base.SaveChangesAsync(cancellationToken);
        }



          
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
            var allTypes = ScanAssemblyTypes();
            var tt       = GetAllEntityTypes(allTypes);



            foreach (Type entityType in tt)
            {
                var entityName     = entityType.Name;
                var entityFullName = entityType.FullName ?? entityName;

                try
                {

                    #region QueryFilters

                    
                    var paramName      = $"{entityName}_p";

                    var entityTypeFromModel = builder.Model.FindEntityType(entityFullName);
                    var queryFilter         = entityTypeFromModel.QueryFilter;
                    var queryParam          = queryFilter?.Parameters.FirstOrDefault();
                    if (null != queryParam)
                    {
                        paramName = queryParam.Name;
                    }

                  

                    ParameterExpression paramExpr = Expression.Parameter(entityType, paramName);

                    //Expression bodyTenant = Expression.Equal(Expression.Property(paramExpr, "TenantId"), 
                    //                                         Expression.Constant(CurrentTenantId)
                    //                                        );

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
                        //body = Expression.AndAlso(bodyDeleted, bodyTenant);
                        body = bodyDeleted;
                    }


                    var name = $"TenantQueryFilter_{entityName}";

                    LambdaExpression lambdaExpr = Expression.Lambda(body, name,
                                                                    new List<ParameterExpression>() { paramExpr }
                                                                   );

                    var entyTYpeBuilder = builder.Entity(entityType);


                    entyTYpeBuilder.HasQueryFilter(lambdaExpr);

                    #endregion

                }
                catch (Exception e)
                {
                    var err = $"Error configuring '{entityName}'";
                    _logger?.LogCritical(err, e);
                    throw new Exception(err, e);
                }

            }
        }


        //[NotNull]
        //protected virtual Type[] ScanAssemblyTypes()
        //{
        //    return GetType().Assembly.GetTypes();
        //}

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

        #region Unit Of Work 

        /// <summary>Begins the transaction.</summary>
        public void BeginTransaction()
        {
            DisposeTransaction();

            var t = Task.Run(async () =>
            {
                Transaction = await Database.BeginTransactionAsync(CancellationToken);
                //var tenant  = await EnsureTenantAsync();

                var tyAudit = new TransactionAudit()
                {
                    Author = Author, UtcDateTime = DateTime.UtcNow, StrIdentifier = Identifier
                };

                var ty = await TransactionAudits.AddAsync(tyAudit, CancellationToken);
                await SaveChangesAsync(true, CancellationToken);

                TransactionAudit = ty.Entity;

            });

            t.Wait(CancellationToken);

           

            
            UowLogger?.LogDebug($"Initialized Uow with {nameof(Identifier)} '{Identifier}'");
            
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
                var d = DateTime.UtcNow - TransactionAudit.UtcDateTime;
                TransactionAudit.MillisecDuration = d.TotalMilliseconds;
                TransactionAudit.StrIdentifier    = Identifier;
            

                if (transactionCommitMessage != "")
                {
                    TransactionAudit.CommitMessage = transactionCommitMessage;
                }

                if (ScopeDictionary.Count > 0)
                {
                    var s = string.Join(" => "
                                        , ScopeDictionary.OrderBy(x => x.Key).Select(x => x.Value).ToArray());
                    if (s.Length > 500)
                    {
                        s = $"{s.Substring(0, 497)}...";
                    }

                    TransactionAudit.Scopes = s;
                }



                var t = Task.Run(async () =>
                {
                    TransactionAudits.Update(TransactionAudit);
                    await SaveChangesAsync(CancellationToken);
                });

                t.Wait(CancellationToken);



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
            return new NamedScope(_logger, scopeName);
        }

        #endregion

        /// <summary>Gets or sets the uow logger.</summary>
        /// <value>The uow logger.</value>
        public ILogger UowLogger { get; set; }


        private ILogger _logger => null == Logger ? UowLogger : Logger;


        
        /// <summary>
        /// Init Method
        /// </summary>
        /// <returns>Instance of initialized Service</returns>
        [NotNull]
        public IdentityBaseContext<TUser, TRole, TKey> Initialize() => InitializeSelf();

        /// <summary>Initializes the self instance.</summary>
        /// <returns></returns>
        [NotNull]
        protected abstract IdentityBaseContext<TUser, TRole, TKey> InitializeSelf();

        /// <summary>Initializes this instance.</summary>
        /// <returns></returns>
        [NotNull]
        IIdentityBaseContext<TUser, TRole, TKey> IIdentityBaseContext<TUser, TRole, TKey>.Initialize()  => InitializeSelf();
        

        class NamedScope : IDisposable
        {
            private IDisposable _scopeDisposable;

            public NamedScope([CanBeNull] ILogger logger, string name)
            {
                _scopeDisposable = logger?.BeginScope(name);
            }

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            public void Dispose()
            {
                _scopeDisposable?.Dispose();
            }
        }





     

    }
}