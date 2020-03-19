using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PH.UowEntityFramework.EntityFramework")]
[assembly: InternalsVisibleTo("PH.UowEntityFramework.EntityFramework.Identity")]
namespace PH.UowEntityFramework.UnitOfWork
{
    /// <summary>
    /// Base Event Argument class
    /// </summary>
    public class UnitOfWorkEventArg : EventArgs
    {
        
        private UnitOfWorkEventArg(string identifier)
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

        /// <summary>Init new Instance with the specified identifier.</summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns>UnitOfWorkEventArg</returns>
        internal static UnitOfWorkEventArg Instance(string identifier) => new UnitOfWorkEventArg(identifier);
        
    }
}