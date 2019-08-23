using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PH.UowEntityFramework.EntityFramework.Audit;

namespace PH.UowEntityFramework.EntityFramework.Infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    /// <typeparam name="TRole">The type of the role.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext{TUser, TRole, TKey}" />
    public abstract class IdentityBaseContextInfrastructure<TUser, TRole, TKey> : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<TUser, TRole, TKey>
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
        public IIdentifier Identifier { get; set; }

        
        /// <summary>Gets or sets the logger.</summary>
        /// <value>The logger.</value>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Current Transaction Audit.
        /// </summary>
        protected TransactionAudit TransactionAudit;

        

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IdentityBaseContext{TUser, TRole, TKey}"/> is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        public bool Initialized { get; protected set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityBaseContextInfrastructure{TUser, TRole, TKey}"/> class.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="T:Microsoft.EntityFrameworkCore.DbContext" />.</param>
        protected IdentityBaseContextInfrastructure([NotNull] DbContextOptions options)
            :base(options)
        {
            Changecount = 0;
            //CurrentTenantSelectedName = DefaultTenantName;
            Initialized = false;
        }

        #region tenants

        ///// <summary>
        ///// The default tenant identifier
        ///// </summary>
        //internal static int DefaultTenantId = 1;

        ///// <summary>The default tenant name</summary>
        //public static string DefaultTenantName = "Default";

        ///// <summary>
        ///// The default tenant name normalized
        ///// </summary>
        //internal static string DefaultTenantNameNormalized = "DEFAULT";

        ///// <summary>Gets or sets the current tenant identifier.</summary>
        ///// <value>The current tenant identifier.</value>
        //public int CurrentTenantId { get; protected set; }
        //protected Tenant CurrentTenant { get; set; }

        ///// <summary>
        ///// Field for detecting current Tenant. Can be set in ctor of DbContext or using <see cref="TenantName" />
        ///// </summary>
        //public string CurrentTenantSelectedName { get; set; }

        ///// <summary>
        ///// Tenant Name Identifier
        ///// </summary>
        //public string TenantName
        //{
        //    get => GetTenantName();
        //    set => SetTenantName(value);
        //}

        ///// <summary>Gets the name of the tenant.</summary>
        ///// <returns></returns>
        //protected virtual string GetTenantName() => CurrentTenantSelectedName;

        ///// <summary>Sets the name of the tenant.</summary>
        ///// <param name="name">The name.</param>
        ///// <returns></returns>
        ///// <exception cref="ArgumentNullException">name</exception>
        //protected virtual void SetTenantName([NotNull] string name)
        //{
        //    if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
        //    {
        //        throw new ArgumentNullException(nameof(name));
        //    }
        //        string n = name.Trim();
        //        CurrentTenantSelectedName = n;
        //        if (Initialized)
        //        {
        //            EnsureTenantAsync().Wait();
        //        }

        //}

        //// <summary>Ensures the tenant asynchronous.</summary>
        ///// <returns></returns>
        ///// <exception cref="Exception">Context not initialized</exception>
        //protected abstract Task<Tenant> EnsureTenantAsync();

        #endregion
        
        /// <summary>Gets or sets the changecount.</summary>
        /// <value>The changecount.</value>
        public int Changecount { get; protected set; }


        protected internal int SacheChangesInternal()
        {
            return base.SaveChanges();
        }
        protected internal async Task<int> SaveChangesInternalAsync(CancellationToken cancellationToken =
                                                                        new CancellationToken() )
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>Saves the base changes.</summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="author">The author.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// identifier
        /// or
        /// author - Author must be set before any savechanges
        /// </exception>
        protected int SaveBaseChanges([NotNull] IIdentifier identifier,[NotNull] string author)
        {
            try
            {
                if (identifier is null)
                {
                    throw new ArgumentNullException(nameof(identifier));
                }

                if (string.IsNullOrEmpty(author) || string.IsNullOrWhiteSpace(author))
                {
                    throw new ArgumentNullException(nameof(author), @"Author must be set before any savechanges");
                }

                var auditEntries = this.OnBeforeSaveChanges(identifier, author);
                var result       = base.SaveChanges();
                var auditsNum    = OnAfterSaveChanges(auditEntries);
                Changecount += result;
                return result;

            }
            catch (Exception e)
            {
                Logger?.LogCritical($"SaveBaseChanges error: {e.Message}", e);

                throw;
            }
            
        }

        /// <summary>Saves the base changes.</summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="author">The author.</param>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// identifier
        /// or
        /// author - Author must be set before any savechanges
        /// </exception>
        protected int SaveBaseChanges([NotNull] IIdentifier identifier,[NotNull] string author, bool b)
        {
            try
            {
                if (identifier is null)
                {
                    throw new ArgumentNullException(nameof(identifier));
                }

                if (string.IsNullOrEmpty(author) || string.IsNullOrWhiteSpace(author))
                {
                    throw new ArgumentNullException(nameof(author), @"Author must be set before any savechanges");
                }

                var auditEntries = OnBeforeSaveChanges(identifier, author);
                var result       = base.SaveChanges(b);
                var auditsNum    = OnAfterSaveChanges(auditEntries);
                Changecount += result;
                return result;


            }
            catch (Exception e)
            {
                Logger?.LogCritical($"SaveBaseChanges error: {e.Message}", e);

                throw;
            }

        }


        /// <summary>Saves the base changes asynchronous.</summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="author">The author.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// identifier
        /// or
        /// author - Author must be set before any savechanges
        /// </exception>
        protected async Task<int> SaveBaseChangesAsync([NotNull] IIdentifier identifier,[NotNull] string author,CancellationToken cancellationToken =
                                                            new CancellationToken())
        {
            try
            {
                if (identifier is null)
                {
                    throw new ArgumentNullException(nameof(identifier));
                }

                if (string.IsNullOrEmpty(author) || string.IsNullOrWhiteSpace(author))
                {
                    throw new ArgumentNullException(nameof(author), @"Author must be set before any savechanges");
                }

                var auditEntries = OnBeforeSaveChanges(identifier, author);
                var result       = await base.SaveChangesAsync(cancellationToken);
                var auditsNum    = await OnAfterSaveChangesAsync(auditEntries);
                Changecount += result;
                return result;

            }
            catch (Exception e)
            {
                Logger?.LogCritical($"SaveBaseChangesAsync error: {e.Message}", e);

                throw;
            }
            
        }

        /// <summary>Saves the base changes asynchronous.</summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="author">The author.</param>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// identifier
        /// or
        /// author - Author must be set before any savechanges
        /// </exception>
        protected async Task<int> SaveBaseChangesAsync([NotNull] IIdentifier identifier,[NotNull] string author, bool b,CancellationToken cancellationToken =
                                                            new CancellationToken())
        {
            try
            {
                if (identifier is null)
                {
                    throw new ArgumentNullException(nameof(identifier));
                }

                if (string.IsNullOrEmpty(author) || string.IsNullOrWhiteSpace(author))
                {
                    throw new ArgumentNullException(nameof(author), @"Author must be set before any savechanges");
                }

                var auditEntries = OnBeforeSaveChanges(identifier, author);
                var result       = await base.SaveChangesAsync(b, cancellationToken);
                var auditsNum    = await OnAfterSaveChangesAsync(auditEntries);
                Changecount += result;
                return result;

            }
            catch (Exception e)
            {
                Logger?.LogCritical($"SaveBaseChangesAsync error: {e.Message}", e);
                throw;
            }
            
        }

        #region Audits





        /// <summary>
        /// Called before save changes.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="author">The author.</param>
        /// <returns></returns>
        [NotNull]
        private List<AuditEntry> OnBeforeSaveChanges([NotNull] IIdentifier identifier, string author)
        {
            try
            {
                ChangeTracker.DetectChanges();
                

                var auditEntries = new List<AuditEntry>();

                var  transactionId = identifier.Uid;
                bool allOk         = true;
                var  l             = new List<CoreValidationException>();

                foreach (var entry in ChangeTracker.Entries())
                {
                    

                    if (entry.Entity is Audit.Audit || entry.Entity is TransactionAudit ||
                        entry.State == EntityState.Detached ||
                        entry.State == EntityState.Unchanged)
                    {
                        continue;
                    }

                
                    if (entry.Entity is IEntity e)
                    {

                        //e.TenantId = CurrentTenantId;
                        

                        if (entry.State == EntityState.Added)
                        {
                            e.CreatedTransactionId = TransactionAudit.Id;
                            e.UpdatedTransactionId = TransactionAudit.Id;
                            e.Deleted              = false;
                        }

                        if (entry.State == EntityState.Modified)
                        {
                            e.UpdatedTransactionId = TransactionAudit.Id;
                            
                        }



                    }


                    if (!DataAnnotationsValidator.TryValidate(entry.Entity, out var errors))
                    {
                        allOk = false;
                        var err = $"Invalid data on '{entry.Entity.GetType().Name}'"; 
                        l.Add(CoreValidationException.ParseDataAnnotationErrors(Identifier, errors, err ));
                    
                    }


                    if (allOk)
                    {
                    
                        var auditEntry = new AuditEntry(entry, transactionId, author)
                        {
                            TableName = entry.Metadata.Relational().TableName
                        };


                        auditEntries.Add(auditEntry);

                        foreach (var property in entry.Properties)
                        {
                            if (property.IsTemporary)
                            {
                                // value will be generated by the database, get the value after saving
                                auditEntry.TemporaryProperties.Add(property);
                                continue;
                            }

                            string propertyName = property.Metadata.Name;
                            if (property.Metadata.IsPrimaryKey())
                            {
                                auditEntry.KeyValues[propertyName] = property.CurrentValue;
                                continue;
                            }

                            switch (entry.State)
                            {
                                case EntityState.Added:
                                    auditEntry.NewValues[propertyName] = property.CurrentValue;
                                    break;

                                case EntityState.Deleted:
                                    auditEntry.OldValues[propertyName] = property.OriginalValue;
                                    break;

                                case EntityState.Modified:
                                    if (property.IsModified)
                                    {
                                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                                    }

                                    break;
                            }
                        }

                    }

                }

                if (!allOk)
                {
                    throw new CoreAggregateException(Identifier, @"Error saving data", l);
                }

                // Save audit entities that have all the modifications
                foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
                {
                    Audits.Add(auditEntry.ToAudit());
                }

                // keep a list of entries where the value of some properties are unknown at this step
                return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
            }
            catch (Exception exc)
            {
                Logger?.LogCritical($"OnBeforeSaveChanges error: {exc.Message}", exc);
                throw;
            }
        }


       

        private int OnAfterSaveChanges([CanBeNull] List<AuditEntry> auditEntries)
        {
            try
            {
                if (auditEntries == null || auditEntries.Count == 0)
                {
                    return 0;
                }


                foreach (var auditEntry in auditEntries)
                {
                    // Get the final value of the temporary properties
                    foreach (var prop in auditEntry.TemporaryProperties)
                    {
                        if (prop.Metadata.IsPrimaryKey())
                        {
                            auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                        }
                        else
                        {
                            auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                        }
                    }

                    // Save the Audit entry

                    Audits.Add(auditEntry.ToAudit());
                }

                return SaveChanges();
            }
            catch (Exception exc)
            {
                Logger?.LogCritical($"OnAfterSaveChanges error: {exc.Message}", exc);

                throw;
            }
        }

        [NotNull]
        private async Task<int> OnAfterSaveChangesAsync([CanBeNull] List<AuditEntry> auditEntries)
        {
            try
            {
                if (auditEntries == null || auditEntries.Count == 0)
                {
                    return 0;
                }


                foreach (var auditEntry in auditEntries)
                {
                    // Get the final value of the temporary properties
                    foreach (var prop in auditEntry.TemporaryProperties)
                    {
                        if (prop.Metadata.IsPrimaryKey())
                        {
                            auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                        }
                        else
                        {
                            auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                        }
                    }

                    // Save the Audit entry
                    Audits.Add(auditEntry.ToAudit());
                }

                return await base.SaveChangesAsync();
            }
            catch (Exception exc)
            {
                Logger?.LogCritical($"OnAfterSaveChangesAsync error: {exc.Message}", exc);

                throw;
            }
        }

        #endregion

    }
}