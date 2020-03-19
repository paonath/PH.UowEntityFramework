using System;
using Microsoft.AspNetCore.Identity;
using PH.UowEntityFramework.EntityFramework.Abstractions.Models;
using PH.UowEntityFramework.EntityFramework.Infrastructure;

namespace PH.UowEntityFramework.EntityFramework.Identity.Infrastructure
{
    /// <summary>
    /// Base interface for Identity Db Context
    /// </summary>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    /// <typeparam name="TRole">The type of the role.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="ITenantContext" />
    public interface IIdentityBaseContext<TUser, TRole, TKey> : IBaseContext
        where TUser : IdentityUser<TKey>, IEntity<TKey> 
        where TRole : IdentityRole<TKey>, IEntity<TKey> 
        where TKey : IEquatable<TKey>
    {
       
        
    }
}