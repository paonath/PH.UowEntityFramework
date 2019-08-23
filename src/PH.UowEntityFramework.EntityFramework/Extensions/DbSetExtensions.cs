using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PH.UowEntityFramework.EntityFramework.Extensions
{
    /// <summary>
    /// Generic Extensions for DbSet
    /// </summary>
    public static class DbSetExtensions
    {
        /// <summary>Updates the entity asynchronous.</summary>
        /// <typeparam name="T">Type of Entity</typeparam>
        /// <param name="dbSet">The database set.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry for the entity.The entry provides access to change tracking information and operations for the entity.</returns>
        [NotNull]
        public static Task<EntityEntry<T>> UpdateAsync<T>([NotNull] this DbSet<T> dbSet, [NotNull] T entity) where T : class 
            => Task.FromResult(dbSet.Update(entity));
    }
}