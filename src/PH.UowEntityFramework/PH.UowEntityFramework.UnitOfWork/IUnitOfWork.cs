using System;
using System.Threading.Tasks;

namespace PH.UowEntityFramework.UnitOfWork
{
    /// <summary>
    /// Unit Of Work
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IUnitOfWork : IDisposable
    {

        Guid BeginTransaction();
        Task<Guid> BeginTransactionAsync();


       
        /// <summary>
        /// Identifier
        /// </summary>
        string Identifier { get; set; }


        /// <summary>
        /// Perform Transaction Commit on a Db.
        /// On Error automatically perform a <see cref="Rollback"/>
        /// </summary>
        void Commit();

        /// <summary>
        /// Perform Transaction Commit on a Db and write a custom log message related to this commit.
        /// On Error automatically perform a <see cref="Rollback"/>
        /// </summary>
        /// <param name="logMessage"></param>
        void Commit(string logMessage);

        /// <summary> Perform Transaction Commit on a Db and write a custom log message related to this commit asynchronous.
        ///
        /// On Error automatically perform a <see cref="Rollback"/>
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <returns></returns>
        Task<int> CommitAsync(string logMessage);

        /// <summary>
        /// Rollback changes on Db Transaction.
        ///
        /// <exception cref="Exception">On rollback error re-trow exception</exception>
        /// </summary>
        void Rollback();

        /// <summary>
        /// Fired On Committed Unit Of Work
        /// </summary>
        event EventHandler<UnitOfWorkEventArg> Committed;

        /// <summary>Begins the scope.</summary>
        /// <param name="scopeName">Name of the scope.</param>
        /// <returns></returns>
        IDisposable BeginScope(string scopeName);
    }
}
