using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PH.UowEntityFramework.EntityFramework.Abstractions.Identity.Models;
using PH.UowEntityFramework.EntityFramework.Abstractions.Models;
using PH.UowEntityFramework.EntityFramework.Identity.Infrastructure;
using PH.UowEntityFramework.EntityFramework.Infrastructure;
using PH.UowEntityFramework.UnitOfWork;

namespace PH.UowEntityFramework.EntityFramework.Identity
{

    //public abstract class BaseContext

    /// <summary>
    /// Identitity Context
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
        /// Initializes a new instance of the <see cref="IdentityBaseContext{TUser, TRole, TKey}"/> class for migration purpouse.
        /// </summary>
        /// <param name="migrationTime">The migration time.</param>
        /// <param name="options">The options.</param>
        protected IdentityBaseContext(DateTime migrationTime,[NotNull] DbContextOptions options)
            :base(migrationTime, options)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityBaseContext{TUser, TRole, TKey}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="auditingEnabled">the audit enable (default false)</param>
        /// <exception cref="ArgumentNullException">options</exception>
        protected IdentityBaseContext([NotNull] DbContextOptions options, bool auditingEnabled = false)
            : base(options, auditingEnabled)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityBaseContext{TUser, TRole, TKey}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="author">The author.</param>
        /// <param name="auditingEnabled">if set to <c>true</c> [auditing enabled].</param>
        protected IdentityBaseContext([NotNull] DbContextOptions options, string identifier, string author, bool auditingEnabled = false)
            : base(options, identifier, author, auditingEnabled)
        {
           
        }



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
        /// Initializes a new instance of the <see cref="IdentityBaseContext{TUser, TRole}"/> class for Migration purpouse.
        /// </summary>
        /// <param name="migrationTime"></param>
        /// <param name="options"></param>
        protected IdentityBaseContext(DateTime migrationTime, DbContextOptions options):base(migrationTime, options)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityBaseContext{TUser, TRole}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="auditingEnabled">the audit enable (default false)</param>
        protected IdentityBaseContext([NotNull] DbContextOptions options, bool auditingEnabled = false)
            : base(options, auditingEnabled)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityBaseContext{TUser, TRole}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="author">The author.</param>
        /// <param name="auditingEnabled">if set to <c>true</c> [auditing enabled].</param>
        protected IdentityBaseContext([NotNull] DbContextOptions options, string identifier, string author, bool auditingEnabled = false)
            : base(options, identifier, author, auditingEnabled)
        {
        }
    }
}