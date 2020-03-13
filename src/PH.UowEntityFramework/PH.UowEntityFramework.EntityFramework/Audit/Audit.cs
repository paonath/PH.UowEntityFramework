using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace PH.UowEntityFramework.EntityFramework.Audit
{
    /// <summary>
    /// Audit Entity: store changes made on all entities
    /// </summary>
    internal class Audit : AuditInfoBase
    {
       

        /// <summary>Creates new values.</summary>
        /// <value>The new values.</value>
        public byte[] Values { get; set; }

      
    }
}