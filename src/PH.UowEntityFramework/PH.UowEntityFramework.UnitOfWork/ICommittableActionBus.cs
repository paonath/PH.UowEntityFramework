using System;

namespace PH.UowEntityFramework.UnitOfWork
{
    /// <summary>
    /// Action Bus related to <see cref="IUnitOfWork"/>: on <see cref="IUnitOfWork.Committed"/> perform flush of enqueued actions (<see cref="Flush"/>)
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ICommittableActionBus : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether throw exception on error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if throw exception on error; otherwise, <c>false</c>.
        /// </value>
        bool ThrowExceptionOnError { get; set; }

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
}