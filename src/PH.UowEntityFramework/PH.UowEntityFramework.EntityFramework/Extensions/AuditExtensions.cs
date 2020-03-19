using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using PH.UowEntityFramework.EntityFramework.Audit;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("PH.UowEntityFramework.EntityFramework.Identity")]
[assembly: InternalsVisibleTo("PH.UowEntityFramework.EntityFramework.Identity.Infrastructure")]
namespace PH.UowEntityFramework.EntityFramework.Extensions
{
    /// <summary>
    /// Extensions for <see cref="Audit"/>
    /// </summary>
    internal static class AuditExtensions
    {
        
        internal static AuditInfo[] ToAuditInfoArray(this Audit.Audit[] audits)
        {
            if (null == audits || audits.Length == 0)
            {
                return new AuditInfo[0];
            }


            var l = new List<AuditInfo>();
            int c = 0;
            foreach (var a in audits)
            {
                string strValue = Encoding.UTF8.GetString(a.Values);

                l.Add(new AuditInfoResult()
                {
                    Id               = a.Id,
                    Author           = a.Author,
                    TableName        = a.TableName,
                    EntityName       = a.EntityName,
                    KeyValue         = a.KeyValue,
                    DateTime         = a.DateTime,
                    Action           = a.Action,
                    TransactionId    = a.TransactionId,
                    JsonStringValues = strValue,
                    Version          = c
                });
                c++;
            }

            return l.OrderByDescending(x => x.DateTime).ToArray();

        }
    }
}