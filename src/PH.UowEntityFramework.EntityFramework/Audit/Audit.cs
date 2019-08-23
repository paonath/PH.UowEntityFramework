using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace PH.UowEntityFramework.EntityFramework.Audit
{
    internal class Audit
    {
        [StringLength(36)]
        public string Id { get; set; }
        public string TableName { get; set; }
        public DateTime DateTime { get; set; }
        public string KeyValues { get; set; }
        public byte[] OldValues { get; set; }
        public byte[] NewValues { get; set; }

        [StringLength(128)]
        public string TransactionId { get; set; }
        [StringLength(255)]
        public string Author { get; set; }
    }
}