using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace PH.UowEntityFramework.EntityFramework.Audit
{
    /// <summary>
    /// Audit Entity: store changes made on all entities
    /// </summary>
    internal class Audit
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        [StringLength(36)]
        public string Id { get; set; }

        /// <summary>Gets or sets the name of the table.</summary>
        /// <value>The name of the table.</value>
        public string TableName { get; set; }

        /// <summary>Gets or sets the date time.</summary>
        /// <value>The date time.</value>
        public DateTime DateTime { get; set; }

        /// <summary>Gets or sets the key values.</summary>
        /// <value>The key values.</value>
        public string KeyValues { get; set; }

        /// <summary>Gets or sets the old values.</summary>
        /// <value>The old values.</value>
        public byte[] OldValues { get; set; }

        /// <summary>Creates new values.</summary>
        /// <value>The new values.</value>
        public byte[] NewValues { get; set; }

        /// <summary>Gets or sets the transaction identifier.</summary>
        /// <value>The transaction identifier.</value>
        public long TransactionId { get; set; }

        /// <summary>Gets or sets the author.</summary>
        /// <value>The author.</value>
        [StringLength(255)]
        public string Author { get; set; }
    }
}