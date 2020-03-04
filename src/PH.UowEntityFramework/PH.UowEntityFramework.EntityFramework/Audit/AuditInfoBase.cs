using System;
using System.ComponentModel.DataAnnotations;

namespace PH.UowEntityFramework.EntityFramework.Audit
{
    /// <summary>
    /// Base Class for Auditing Entities
    /// </summary>
    public abstract class AuditInfoBase
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        [StringLength(36)]
        public string Id { get; set; }

        /// <summary>Gets or sets the action.</summary>
        /// <value>The action.</value>
        [StringLength(10)]
        public string Action { get; set; }

        /// <summary>Gets or sets the name of the entity.</summary>
        /// <value>The name of the entity.</value>
        public string TableName { get; set; }

        /// <summary>Gets or sets the name of the entity.</summary>
        /// <value>The name of the entity.</value>
        public string EntityName { get; set; }

        /// <summary>Gets or sets the date time.</summary>
        /// <value>The date time.</value>
        public DateTime DateTime { get; set; }

        /// <summary>Gets or sets the key values.</summary>
        /// <value>The key values.</value>
        public string KeyValue { get; set; }

        /// <summary>Gets or sets the transaction identifier.</summary>
        /// <value>The transaction identifier.</value>
        public long TransactionId { get; set; }

        /// <summary>Gets or sets the author.</summary>
        /// <value>The author.</value>
        [StringLength(255)]
        public string Author { get; set; }
        
    }
}