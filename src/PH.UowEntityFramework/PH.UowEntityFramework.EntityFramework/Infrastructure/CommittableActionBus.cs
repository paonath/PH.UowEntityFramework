using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PH.UowEntityFramework.UnitOfWork;

namespace PH.UowEntityFramework.EntityFramework.Infrastructure
{
    /// <summary>
    /// Action Bus related to <see cref="IUnitOfWork"/>: on <see cref="IUnitOfWork.Committed"/> perform flush of enqueued actions (<see cref="Flush"/>)
    /// </summary>
    /// <seealso cref="PH.UowEntityFramework.UnitOfWork.ICommittableActionBus" />
    public class CommittableActionBus : ICommittableActionBus
    {
        private readonly IUnitOfWork _uow;

        private readonly Queue<Action> _actions;
        private readonly ILogger<CommittableActionBus> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommittableActionBus"/> class.
        /// </summary>
        /// <param name="uow">The uow.</param>
        /// <param name="logger">The logger.</param>
        public CommittableActionBus(IUnitOfWork uow, ILogger<CommittableActionBus> logger)
        {
            _uow = uow;
            _uow.Committed += UowOnCommitted;
            _logger = logger;
            ThrowExceptionOnError = false;
            _actions = new Queue<Action>();
        }

        private void UowOnCommitted(object sender, UnitOfWorkEventArg e)
        {
            Flush();
        }

        /// <summary>
        /// Gets or sets a value indicating whether throw exception on error.
        /// </summary>
        /// <value>
        /// <c>true</c> if throw exception on error; otherwise, <c>false</c>.
        /// </value>
        public bool ThrowExceptionOnError { get; set; }

        /// <summary>Gets the count of queued actions.</summary>
        /// <value>The count.</value>
        public int Count => _actions.Count;

        /// <summary>Enqueues the specified action.</summary>
        /// <param name="action">The action.</param>
        public void Enqueue(Action action)
        {
            _actions.Enqueue(action);
        }

        /// <summary>Flush all actions.</summary>
        /// <param name="throwExceptionOnError">if set to <c>true</c>[throw exception on error.</param>
        /// <exception cref="Exception">if action on error and throwExceptionOnError is set to true</exception>
        /// <returns></returns>
        public virtual void Flush(bool throwExceptionOnError = false)
        {
            
            while (_actions.Count > 0)
            {
                var act = _actions.Dequeue();
                try
                {
                    act.Invoke();
                }
                catch (Exception e)
                {
                    if (throwExceptionOnError)
                    {
                        _logger.LogCritical(e, $"Error performing action: {e.Message}");
                        throw;
                    }
                    else
                    {
                        _logger.LogError(e, $"Error performing action: {e.Message}");
                    }
                }
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}