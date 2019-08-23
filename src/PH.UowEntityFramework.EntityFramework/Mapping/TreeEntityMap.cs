using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PH.UowEntityFramework.EntityFramework.Mapping
{
    /// <inheritdoc cref="IEntityMap{TEntity,TKey}" />
    public abstract class TreeEntityMap<TEntity, TKey> : EntityMap<TEntity, TKey>, IEntityMap<TEntity, TKey>, IEntityTypeConfiguration<TEntity>
        where TEntity : class, ITreeEntity<TEntity, TKey>
        where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        ///     Configures the entity of type <typeparamref name="TEntity" />.
        /// </summary>
        /// <param name="builder"> The builder to be used to configure the entity type. </param>
        public override void Configure([NotNull] EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.EntityLevel)
                   .HasColumnName("EntityLevelLevel")
                   .IsRequired(true);


            builder.HasOne(x => x.Parent)
                   .WithMany(x => x.Childrens)
                   .HasForeignKey(x => x.ParentId)
                   .IsRequired(false);


            builder
                .HasIndex(i => new
                {
                    i.EntityLevel,
                    i.RootId,
                    i.ParentId
                }).IsUnique(false);

        }
    }
}