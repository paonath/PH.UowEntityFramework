using System;

namespace PH.UowEntityFramework.UnitOfWork
{
    /// <summary>
    /// Action Bus related to <see cref="IUnitOfWork"/>: on <see cref="IUnitOfWork.Committed"/> perform flush of enqueued actions (<see cref="Flush"/>)
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ICommittableActionBus : IDisposable
    {
        /// <summary>Gets the count of queued actions.</summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>Enqueues the specified action.</summary>
        /// <param name="action">The action.</param>
        void Enqueue(Action action);

        /// <summary>Flush all actions.</summary>
        /// <param name="throwExceptionOnError">if set to <c>true</c>throw exception on error.</param>
        void Flush(bool throwExceptionOnError = false);
    }


    public interface IUnitOfWork : IDisposable
    {
        bool Disposed { get; }
        
        string Uid { get; }


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
