using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using PH.UowEntityFramework.TestCtx.Models;
using PH.UowEntityFramework.TestCtx.Models.Maps;

namespace PH.UowEntityFramework.TestCtx
{
    public class DebugCtx : PH.UowEntityFramework.EntityFramework.IdentityBaseContext<UserDebug,RoleDebug>
    {
        public DbSet<DataDebug> MyData { get; set; }
        public DbSet<NodeDebug> Nodes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public DebugCtx([NotNull] DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Custom On Model Creating
        /// </summary>
        /// <param name="builder">The builder being used to construct the model for this context.</param>
        public override void OnCustomModelCreating(ModelBuilder builder)
        {
            //
            builder.ApplyConfiguration(new DataDebugMap());
            builder.ApplyConfiguration(new NodeDebugMap());

            builder.ApplyConfiguration(new UserDebugMap());
            builder.ApplyConfiguration(new RoleDebugMap());
        }

        /// <summary>Scans the assembly types.</summary>
        /// <returns></returns>
        protected override Type[] ScanAssemblyTypes()
        {
            return typeof(UserDebug).Assembly.GetTypes();
        }
    }
}
