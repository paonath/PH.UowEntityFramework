using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PH.UowEntityFramework.TestCtx.Models.Maps
{
    internal class DataDebugMap : PH.UowEntityFramework.EntityFramework.Mapping.EntityMap<DataDebug, string>
    {
        /// <summary>
        ///     Configures the entity of type <typeparamref name="TEntity" />.
        ///
        /// Override Configure Method calling base!
        /// </summary>
        /// <param name="builder"> The builder to be used to configure the entity type. </param>
        public override void Configure(EntityTypeBuilder<DataDebug> builder)
        {
            base.Configure(builder);

            builder.ToTable("Data");

            builder.HasOne(x => x.Author)
                   .WithMany(x => x.GeneratedData)
                   .HasForeignKey(x => x.AuthorId)
                   .IsRequired(true);
        }
    }
}