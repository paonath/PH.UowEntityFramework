using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
using PH.UowEntityFramework.EntityFramework.Infrastructure;
using PH.UowEntityFramework.EntityFramework.Mapping;
using PH.UowEntityFramework.UnitOfWork;
using PH.UowEntityFramework.EntityFramework.Extensions;

namespace PH.UowEntityFramework.EntityFramework
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
        /// 
        /// </summary>
        /// <param name="migrationTime"></param>
        /// <param name="options"></param>
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

        /// <summary>Gets the uid.</summary>
        /// <value>The uid.</value>
        public string Uid => Identifier;

        /// <summary>Finds the audit information.</summary>
        /// <param name="id">The audit identifier.</param>
        /// <returns></returns>
        public async Task<Audit.AuditInfo> FindAuditInfoAsync(string id)
        {
            if (!AuditingEnabled)
            {
                return null;
            }

            var i = await Audits.FirstOrDefaultAsync(x => x.Id == id);
            
            return i.ToAuditInfo();
        }

        /// <summary>Finds the audit information for a entity.</summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityKey">The type of the entity key.</typeparam>
        /// <param name="id">The entity identifier.</param>
        /// <returns></returns>
        [ItemCanBeNull]
        public async Task<AuditInfo[]> FindAuditInfoAsync<TEntity, TEntityKey>(TEntityKey id) 
            where TEntity : class, IEntity<TEntityKey> where TEntityKey : IEquatable<TEntityKey>
        {
            
            //if (!AuditingEnabled)
            //{
            //    return null;
            //}

            //var entry = await this.Set<TEntity>().FirstOrDefaultAsync(x => x.Id.Equals(id));
            //if (null == entry)
            //{
            //    return null;
            //}

            ////var tbl = Model.FindEntityType(typeof(TEntity)).Relational().TableName;
            //var tbl = Model.FindEntityType(typeof(TEntity)).GetTableName();

            //var iid = ("{\"Id\":\"" + $"{id}" + "\"}");

            //var audits = await Audits.Where(x => x.TableName == tbl && x.KeyValues == iid).OrderBy(x => x.DateTime)
            //                         .ToArrayAsync();

            //return audits.Select(x => x.ToAuditInfo()).ToArray();

            throw new NotImplementedException("refactor");

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

        public IdentityBaseContext(DateTime migrationTime, DbContextOptions options):base(migrationTime, options)
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