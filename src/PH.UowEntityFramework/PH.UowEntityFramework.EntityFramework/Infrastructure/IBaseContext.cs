using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PH.UowEntityFramework.EntityFramework.Infrastructure
{
    /// <summary>
    /// Base Interface for Db Context
    /// </summary>
    public interface IBaseContext
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
        string Identifier { get; set; }

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