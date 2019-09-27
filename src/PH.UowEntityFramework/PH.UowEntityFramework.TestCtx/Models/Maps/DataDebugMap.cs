using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PH.UowEntityFramework.EntityFramework.Mapping;

namespace PH.UowEntityFramework.TestCtx.Models.Maps
{
    internal class DataDebugMap : PH.UowEntityFramework.EntityFramework.Mapping.EntityMap<DataDebug, string>
    {
        /// <summary>
        ///     
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

    internal class NodeDebugMap : EntityMap<NodeDebug, string>
    {
        /// <summary>
        ///     
        ///
        /// Override Configure Method calling base!
        /// </summary>
        /// <param name="builder"> The builder to be used to configure the entity type. </param>
        public override void Configure(EntityTypeBuilder<NodeDebug> builder)
        {
            base.Configure(builder);

            builder.ToTable("nodes_test");

            builder.HasOne(x => x.Parent)
                   .WithMany(x => x.Children)
                   .HasForeignKey(x => x.ParentId)
                   .IsRequired(false);

            builder.HasOne(x => x.Data)
                   .WithMany(x => x.Nodes)
                   .HasForeignKey(x => x.DataId)
                   .IsRequired(false);

        }
    }
}