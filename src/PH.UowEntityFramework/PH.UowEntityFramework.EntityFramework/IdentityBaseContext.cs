﻿using System;
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
using PH.UowEntityFramework.EntityFramework.Infrastructure;
using PH.UowEntityFramework.EntityFramework.Mapping;
using PH.UowEntityFramework.UnitOfWork;
using PH.UowEntityFramework.EntityFramework.Extensions;

namespace PH.UowEntityFramework.EntityFramework
{
    /// <summary>
    /// Core Identitity Context
    ///
    /// <see cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext{TUser, TRole, TKey}"/>
    /// </summary>
    /// <typeparam name="TUser">Type of User Entity class</typeparam>
    /// <typeparam name="TRole">Type of Role Entity class</typeparam>
    /// <typeparam name="TKey">Type of User and Role Id Property</typeparam>
    public abstract class IdentityBaseContext<TUser, TRole, TKey> :
        IdentityBaseContextInfrastructure<TUser, TRole, TKey> , IDbContextUnitOfWork , IUnitOfWork
        where TUser : IdentityUser<TKey>, IEntity<TKey>
        where TRole : IdentityRole<TKey>, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
       




        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityBaseContext{TUser, TRole, TKey}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        protected IdentityBaseContext([NotNull] DbContextOptions options)
            : base(options)
        {
           
        }


        /// <summary>Gets the uid.</summary>
        /// <value>The uid.</value>
        public string Uid => Identifier;

        

        #region SaveChanges



        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>
        /// The number of state entries written to the database.
        /// </returns>
        /// <remarks>
        /// This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
        /// changes to entity instances before saving to the underlying database. This can be disabled via
        /// <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
        /// </remarks>
        public sealed override int SaveChanges()
        {
            var auditEntries = this.OnBeforeSaveChanges(Audits); 
            var result       = base.SaveChanges();
            this.OnAfterSaveChanges(Audits,auditEntries);
            Changecount += result;
            return result;
        }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">Indicates whether <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after the changes have
        /// been sent successfully to the database.</param>
        /// <returns>
        /// The number of state entries written to the database.
        /// </returns>
        /// <remarks>
        /// This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
        /// changes to entity instances before saving to the underlying database. This can be disabled via
        /// <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
        /// </remarks>
        public sealed override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var auditEntries = this.OnBeforeSaveChanges(Audits); 
            var result       = base.SaveChanges(acceptAllChangesOnSuccess);
            this.OnAfterSaveChanges(Audits,auditEntries);
            Changecount += result;
            return result;
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains the
        /// number of state entries written to the database.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
        /// changes to entity instances before saving to the underlying database. This can be disabled via
        /// <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
        /// </para>
        /// <para>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </para>
        /// </remarks>
        public sealed override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = new CancellationToken()) =>
            await SaveChangesAsync(true, cancellationToken);
        

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">Indicates whether <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after the changes have
        /// been sent successfully to the database.</param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains the
        /// number of state entries written to the database.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
        /// changes to entity instances before saving to the underlying database. This can be disabled via
        /// <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
        /// </para>
        /// <para>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </para>
        /// </remarks>
        public sealed override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            var auditEntries = this.OnBeforeSaveChanges(Audits);
            var result       = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await this.OnAfterSaveChangesAsync(Audits,auditEntries);
            Changecount += result;
            return result;
        }

        #endregion


        //[NotNull]
        //public IDbContextUnitOfWork Initialize() => InitializeSelf();

        /// <summary>Initializes the self instance.</summary>
        /// <returns></returns>
        protected override IdentityBaseContext<TUser, TRole, TKey> InitializeSelf()
        {
            if (!Initialized)
            {
                Initialized = true;
                BeginTransaction();

            }

            
            return this;
        }


        /// <summary>Initializes this instance.</summary>
        /// <returns>IDbContextUnitOfWork instance initialized</returns>
        [NotNull]
        public new IDbContextUnitOfWork Initialize() => InitializeSelf();
        
    }


    /// <summary>
    ///  Core Identitity Context
    /// </summary>
    /// <typeparam name="TUser">Type of User Entity class</typeparam>
    /// <typeparam name="TRole">Type of Role Entity class</typeparam>
    public abstract class
        IdentityBaseContext<TUser, TRole> : IdentityBaseContext<TUser, TRole, string> , IDbContextUnitOfWork , IUnitOfWork
        where TUser : UserEntity, IEntity<string>
        where TRole : RoleEntity, IEntity<string>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        protected IdentityBaseContext([NotNull] DbContextOptions options)
            : base(options)
        {
        }
    }
}