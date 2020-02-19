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

        public DebugCtx(DateTime migrationTime, DbContextOptions options)
            :base(migrationTime, options)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugCtx"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="auditingEnabled">the audit enable (default false)</param>
        public DebugCtx([NotNull] DbContextOptions options, bool auditingEnabled = false) : base(options, auditingEnabled)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugCtx"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="author">The author.</param>
        /// <param name="auditingEnabled">if set to <c>true</c> [auditing enabled].</param>
        public DebugCtx([NotNull] DbContextOptions options, string identifier, string author, bool auditingEnabled = false) 
            : base(options, identifier, author,auditingEnabled)
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
