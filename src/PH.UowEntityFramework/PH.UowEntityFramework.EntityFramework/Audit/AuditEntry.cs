using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using MassTransit;
using Microsoft.EntityFrameworkCore.ChangeTracking;


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
            byte[] old = null;
            if(OldValues.Count > 0)
            {

                //var serializedData =
                //    JsonSerializer.Serialize(OldValues, new JsonSerializerOptions() {WriteIndented = false});

                ////old = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(OldValues, Formatting.None, new JsonSerializerSettings(){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore } ));
                //old = Encoding.UTF8.GetBytes(serializedData);
                old = JsonSerializer.SerializeToUtf8Bytes(OldValues,
                                                          new JsonSerializerOptions() {WriteIndented = false});
            }

            byte[] add = null;
            if (NewValues.Count > 0)
            {
                //add = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(NewValues, Formatting.None, new JsonSerializerSettings(){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore } ));
                //var serializedData =
                //    JsonSerializer.Serialize(NewValues, new JsonSerializerOptions() {WriteIndented = false});

                ////old = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(OldValues, Formatting.None, new JsonSerializerSettings(){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore } ));
                //add = Encoding.UTF8.GetBytes(serializedData);

                add = JsonSerializer.SerializeToUtf8Bytes(NewValues,
                                                          new JsonSerializerOptions() {WriteIndented = false});

            }

            

            var audit = new Audit
            {
                TableName     = TableName,
                DateTime      = DateTime.UtcNow,
                KeyValues     = JsonSerializer.Serialize(KeyValues),
                OldValues     = old,
                NewValues     = add,
                TransactionId = TransactionId,
                Author        = Author,
                Id            = $"{NewId.Next()}"
            };

            return audit;
        }
    }
}