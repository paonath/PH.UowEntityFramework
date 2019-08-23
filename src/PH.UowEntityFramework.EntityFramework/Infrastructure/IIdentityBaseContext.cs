﻿using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PH.UowEntityFramework.EntityFramework.Infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    /// <typeparam name="TRole">The type of the role.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="ITenantContext" />
    public interface IIdentityBaseContext<TUser, TRole, TKey> : /*ITenantContext,*/ IInitializable<IdentityBaseContext<TUser, TRole, TKey>>
        where TUser : IdentityUser<TKey>, IEntity<TKey> 
        where TRole : IdentityRole<TKey>, IEntity<TKey> 
        where TKey : IEquatable<TKey>
    {
        /// <summary>Gets or sets the logger.</summary>
        /// <value>The logger.</value>
        ILogger Logger { get; set; }

        /// <summary>Gets the scope dictionary.</summary>
        /// <value>The scope dictionary.</value>
        Dictionary<int, string> ScopeDictionary { get; }

        

        /// <summary>
        /// Identifier
        /// </summary>
        IIdentifier Identifier { get; set; }

        /// <summary>Gets or sets the author.</summary>
        /// <value>The author.</value>
        string Author { get; set; }

        /// <summary>
        /// Ctx Uid
        /// </summary>
        Guid CtxUid { get; }

        /// <summary>Gets or sets the cancellation token source.</summary>
        /// <value>The cancellation token source.</value>
        CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>Gets the cancellation token.</summary>
        /// <value>The cancellation token.</value>
        CancellationToken CancellationToken { get; }



        /// <summary>
        /// Called when [custom model creating].
        /// </summary>
        /// <param name="builder">The builder.</param>
        void OnCustomModelCreating([NotNull] ModelBuilder builder);

        /// <summary>Begins the scope.</summary>
        /// <param name="scopeName">Name of the scope.</param>
        /// <returns></returns>
        IDisposable BeginScope([NotNull] string scopeName);


    }
}