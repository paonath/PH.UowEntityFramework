//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using PH.Core3.EntityFramework.Abstractions.Models.Entities;

//namespace PH.Core3.EntityFramework.Mapping
//{
//    internal class TenantMap : IEntityTypeConfiguration<Tenant>
//    {
//        /// <summary>
//        ///     Configures the entity of type Tenant />.
//        /// </summary>
//        /// <param name="builder"> The builder to be used to configure the entity type. </param>
//        public void Configure(EntityTypeBuilder<Tenant> builder)
//        {
//            builder.ToTable("tenant");

//            builder.Property(x => x.Id);
//            builder.Property(x => x.Name).IsRequired(true);

//            builder.HasIndex(i => i.Name).IsUnique(true);
//            builder.HasIndex(i => i.NormalizedName).IsUnique(true);

//            builder
//                .HasIndex(i => new
//                {
//                    i.Id,
//                    i.NormalizedName
//                });
//        }
//    }
//}