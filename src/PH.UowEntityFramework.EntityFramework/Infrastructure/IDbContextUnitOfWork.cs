using Microsoft.Extensions.Logging;

namespace PH.UowEntityFramework.EntityFramework.Infrastructure
{
    /// <summary>
    /// Db Context Unit Of Work
    /// </summary>
    /// <seealso cref="PH.Core3.UnitOfWork.IUnitOfWork" />
    public interface IDbContextUnitOfWork : IUnitOfWork
    {
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
    }
}