using System;

namespace PH.UowEntityFramework.UnitOfWork
{
    /// <summary>
    /// Base Event Argument class
    /// </summary>
    public abstract class UnitOfWorkEventArg : EventArgs
    {
        
        internal UnitOfWorkEventArg(string identifier)
        {
            Identifier = identifier;
            Id         = Guid.NewGuid();
            UtcFired   = DateTime.UtcNow;
        }

        /// <summary>Gets the identifier.</summary>
        /// <value>The identifier.</value>
        public string Identifier { get; }

        /// <summary>Gets the UTC fired.</summary>
        /// <value>The UTC fired.</value>
        public DateTime UtcFired { get; }

        /// <summary>Gets the identifier.</summary>
        /// <value>The identifier.</value>
        public Guid Id { get; }
    }
}