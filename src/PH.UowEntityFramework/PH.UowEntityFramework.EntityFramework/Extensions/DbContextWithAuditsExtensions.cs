﻿//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Threading.Tasks;
//using JetBrains.Annotations;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.ChangeTracking;
//using Microsoft.Extensions.Logging;
//using PH.UowEntityFramework.EntityFramework.Abstractions.Models;
//using PH.UowEntityFramework.EntityFramework.Audit;
//using PH.UowEntityFramework.EntityFramework.Infrastructure;

//namespace PH.UowEntityFramework.EntityFramework.Extensions
//{
//    /// <summary>
//    /// Extensions methods for DbContext With Audits: <see cref="IdentityBaseContext{TUser,TRole,TKey}"/> 
//    /// </summary>
//    public static class DbContextWithAuditsExtensions
//    {
        


//        [NotNull]
//        internal static List<AuditEntry> OnBeforeSaveChanges<TContext>([NotNull] this TContext context, bool auditingEnabled,
//                                                                       DbSet<Audit.Audit> audits, ILogger logger)
//            where TContext : DbContext, IAuditContext
//        {
//            if (null == context)
//            {
//                throw new ArgumentNullException(nameof(context));
//            }

            

//            context.ChangeTracker.DetectChanges();
//            var entries = context.ChangeTracker.Entries().ToArray();

//            var auditEntries = new List<AuditEntry>();


//            foreach (var entry in entries)
//            {
//                try
//                {
//                    if (entry.Entity is Audit.Audit || entry.Entity is TransactionAudit ||
//                        entry.State == EntityState.Detached ||
//                        entry.State == EntityState.Unchanged)
//                    {
//                        continue;
//                    }
                    
                    
//                    if (auditingEnabled)
//                    {
//                        var auditEntry = new AuditEntry(entry, context.TransactionAudit.Id, context.Author)
//                        {

//                            TableName = entry.Metadata.GetTableName()

//                        };


//                        auditEntries.Add(auditEntry);

//                        foreach (var property in entry.Properties)
//                        {
//                            if (property.IsTemporary)
//                            {
//                                // value will be generated by the database, get the value after saving
//                                auditEntry.TemporaryProperties.Add(property);
//                                continue;
//                            }

//                            string propertyName = property.Metadata.Name;
//                            if (property.Metadata.IsPrimaryKey())
//                            {
//                                auditEntry.KeyValues[propertyName] = property.CurrentValue;
//                                continue;
//                            }

//                            switch (entry.State)
//                            {
//                                case EntityState.Added:
//                                    auditEntry.NewValues[propertyName] = property.CurrentValue;
//                                    break;

//                                case EntityState.Deleted:
//                                    auditEntry.OldValues[propertyName] = property.OriginalValue;
//                                    break;

//                                case EntityState.Modified:
//                                    if (property.IsModified)
//                                    {
//                                        auditEntry.OldValues[propertyName] = property.OriginalValue;
//                                        auditEntry.NewValues[propertyName] = property.CurrentValue;
//                                    }

//                                    break;
//                            }
//                        }
//                    }
                   
//                }
//                catch (Exception exception)
//                {
//                    logger?.LogCritical(exception, $"Error OnBeforeSaveChanges: {exception.Message}");
//                    throw;
//                }
//            }

//            if (auditingEnabled)
//            {
//                // Save audit entities that have all the modifications
//                foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
//                {
//                    audits.Add(auditEntry.ToAudit());
//                }
//            }
            

//            // keep a list of entries where the value of some properties are unknown at this step
//            return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
//        }


//        internal static int OnAfterSaveChanges<TContext>([NotNull] this TContext context, bool auditingEnabled, DbSet<Audit.Audit> audits,
//                                                         [CanBeNull] List<AuditEntry> auditEntries, ILogger logger)
//            where TContext : DbContext, IAuditContext
//        {
//            if (!auditingEnabled)
//            {
//                return 0;
//            }


//            try
//            {
//                if (null == context)
//                {
//                    throw new ArgumentNullException(nameof(context));
//                }

//                if (auditEntries == null || auditEntries.Count == 0)
//                {
//                    return 0;
//                }

//                OnAfterSaveChangesPrivate(context, auditEntries, audits);


//                return context.SaveChanges();
//            }
//            catch (Exception e)
//            {
//                logger?.LogCritical(e, $"Error OnAfterSaveChanges: {e.Message}");
//                throw;
//            }
//        }

//        private static void OnAfterSaveChangesPrivate<TContext>([NotNull] this TContext context, 
//                                                                [CanBeNull] List<AuditEntry> auditEntries,
//                                                                [NotNull] DbSet<Audit.Audit> audits)
//            where TContext : DbContext, IAuditContext
//        {
//            if (auditEntries != null)
//            {
//                foreach (var auditEntry in auditEntries)
//                {
//                    // Get the final value of the temporary properties
//                    foreach (var prop in auditEntry.TemporaryProperties)
//                    {
//                        if (prop.Metadata.IsPrimaryKey())
//                        {
//                            auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
//                        }
//                        else
//                        {
//                            auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
//                        }
//                    }

//                    // Save the Audit entry

//                    audits.Add(auditEntry.ToAudit());
//                }
//            }
//        }

//        [NotNull]
//        internal static async Task<int> OnAfterSaveChangesAsync<TContext>([NotNull] this TContext context, bool auditingEnabled, [NotNull] DbSet<Audit.Audit> audits,
//            [CanBeNull] List<AuditEntry> auditEntries, ILogger logger)
//            where TContext : DbContext, IAuditContext
//        {
//            if (!auditingEnabled)
//            {
//                return 0;
//            }

//            try
//            {
//                if (null == context)
//                {
//                    throw new ArgumentNullException(nameof(context));
//                }

//                if (auditEntries == null || auditEntries.Count == 0)
//                {
//                    return 0;
//                }

//                OnAfterSaveChangesPrivate(context, auditEntries, audits);

//                return await context.SaveChangesAsync();
//            }
//            catch (Exception e)
//            {
//                logger?.LogCritical(e, $"Error OnAfterSaveChangesAsync: {e.Message}");
//                throw;
//            }
//        }
//    }
//}