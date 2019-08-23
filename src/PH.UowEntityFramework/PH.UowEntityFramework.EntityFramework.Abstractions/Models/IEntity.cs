using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PH.UowEntityFramework.EntityFramework.Abstractions.Models
{
    /// <summary>
    /// a Businnes-Logic related Entity.
    ///
    /// All entities of business logic that need to persist on the database must implement the current interface and <see cref="IEntity{TKey}"/>
    ///
    /// <seealso cref="Entity{TKey}"/>
    /// </summary>
    public interface IEntity : ICommonEntityBase
    {
        /// <summary>
        /// Row Version and Concurrency Check Token
        /// </summary>
        [Timestamp]
        byte[] Timestamp { get; set; }

        /// <summary>
        /// True if Entity is Deleted
        ///
        /// (Logical delete)
        /// </summary>
        [Required]
        bool Deleted { get; set; }

        

        /// <summary>
        ///  Uid If <see cref="Deleted"/> 
        /// </summary>
        long? DeletedTransactionId { get; set; }

        /// <summary>
        /// Reference to Delete <see cref="TransactionAudit"/> 
        /// </summary>
        [ForeignKey("DeletedTransactionId")]
        TransactionAudit DeletedTransaction { get; set; }
        


        /// <summary>
        /// Uid of creation
        /// </summary>
        long CreatedTransactionId { get; set; }

        /// <summary>
        /// Reference to Create <see cref="TransactionAudit"/>
        /// </summary>
        [ForeignKey("CreatedTransactionId")]
        TransactionAudit CreatedTransaction { get; set; }

        /// <summary>
        /// Uid of update
        /// </summary>
        long UpdatedTransactionId { get; set; }

        /// <summary>
        /// Reference to Update <see cref="TransactionAudit"/>
        /// </summary>
        [ForeignKey("UpdatedTransactionId")]
        TransactionAudit UpdatedTransaction { get; set; }

    }

    /// <summary>
    /// Entity Base Interface to Persist on Db with Primary Key: <see cref="Id"/>
    ///
    /// All entities that need to persist on the database must implement the current interface
    /// </summary>
    /// <typeparam name="TKey">Type of the Id property</typeparam>
    public interface IEntity<TKey> : IEntity 
        where TKey : IEquatable<TKey>
    {
        /// <summary>Gets or sets the unique id (Primary Key) for current Entity.</summary>
        /// <value>The Id.</value>
        TKey Id { get; set; }
    }
}