using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using MassTransit;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;


[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace PH.UowEntityFramework.EntityFramework.Audit
{
    internal class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public AuditEntry(EntityEntry entry, long transactionId) : this(entry)
        {
            TransactionId = transactionId;
        }
        public AuditEntry(EntityEntry entry, long transactionId,string author) : this(entry,transactionId)
        {
            Author = author;
        }



        public long TransactionId { get; set; }
        public string Author { get; set; }
        public EntityEntry Entry { get; }
        public string TableName { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        [NotNull]
        public Audit ToAudit()
        {
            var settings = new JsonSerializerSettings()
                {ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.None,};
            byte[] old = null;
            if(OldValues.Count > 0)
            {

                
                old = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(OldValues, settings));
               
            }

            byte[] add = null;
            if (NewValues.Count > 0)
            {
               

               

                add = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(NewValues, settings));

            }

            

            var audit = new Audit
            {
                TableName     = TableName,
                DateTime      = DateTime.UtcNow,
                KeyValues     = JsonConvert.SerializeObject(KeyValues, settings),
                OldValues     = old,
                NewValues     = add,
                TransactionId = TransactionId,
                Author        = Author,
                Id            = $"{Guid.NewGuid():N}"
            };

            return audit;
        }
    }
}