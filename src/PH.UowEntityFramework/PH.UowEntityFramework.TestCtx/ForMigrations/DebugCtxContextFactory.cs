using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PH.UowEntityFramework.TestCtx.ForMigrations
{
    public class DebugCtxContextFactory : IDesignTimeDbContextFactory<DebugCtx>
    {
        /// <summary>Creates a new instance of a derived context.</summary>
        /// <param name="args"> Arguments provided by the design-time service. </param>
        /// <returns> An instance of <typeparamref name="TContext" />. </returns>
        public DebugCtx CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DebugCtx>();
            optionsBuilder.UseLazyLoadingProxies();

            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS01;Database=Dbg3;User Id=sa;Password=sa;MultipleActiveResultSets=true");

            return new DebugCtx(optionsBuilder.Options);
        }
    }
}