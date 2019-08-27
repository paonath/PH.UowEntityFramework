using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PH.UowEntityFramework.EntityFramework.Abstractions.Models;

namespace PH.UowEntityFramework.EntityFramework.Mapping
{
    internal class TransactionAuditMap : IEntityTypeConfiguration<TransactionAudit>
    {
        /// <inheritdoc />
        public void Configure([NotNull] EntityTypeBuilder<TransactionAudit> builder)
        {
            builder.ToTable("transaction_audit");

            
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.UtcDateTime).IsRequired();
            builder.Property(x => x.Author).IsRequired();
            
            builder.Property(x => x.MillisecDuration).HasDefaultValue(0);
            builder.Property(x => x.StrIdentifier).IsRequired(true);


            builder
                .HasIndex(i => new
                {
                    i.Id,
                    i.StrIdentifier,
                    i.Author,
                    i.UtcDateTime,
                    i.Timestamp
                });
        }
    }
}