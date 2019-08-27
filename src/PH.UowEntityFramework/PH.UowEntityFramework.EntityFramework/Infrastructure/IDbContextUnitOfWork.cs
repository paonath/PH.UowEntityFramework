using System;
using Microsoft.Extensions.Logging;
using PH.UowEntityFramework.UnitOfWork;

namespace PH.UowEntityFramework.EntityFramework.Infrastructure
{
    /// <summary>
    /// Db Context Unit Of Work
    /// </summary>
    /// <seealso cref="IUnitOfWork" />
    public interface IDbContextUnitOfWork : IDisposable
    {
        /// <summary>
        /// Identifier
        /// </summary>
        string Identifier { get; set; }

        /// <summary>Gets or sets the uow logger.</summary>
        /// <value>The uow logger.</value>
        ILogger UowLogger { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDbContextUnitOfWork"/> is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        bool Initialized { get; }

        /// <summary>Initializes this instance.</summary>
        /// <returns>IDbContextUnitOfWork instance initialized</returns>
        IDbContextUnitOfWork Initialize();

        /// <summary>
        /// Fired On Committed Unit Of Work
        /// </summary>
        event EventHandler<UnitOfWorkEventArg> Committed;

    }
}