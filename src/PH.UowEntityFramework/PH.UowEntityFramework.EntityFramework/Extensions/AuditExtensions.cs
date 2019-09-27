using System.Text;
using JetBrains.Annotations;
using PH.UowEntityFramework.EntityFramework.Audit;

namespace PH.UowEntityFramework.EntityFramework.Extensions
{
    /// <summary>
    /// Extensions for <see cref="Audit"/>
    /// </summary>
    internal static class AuditExtensions
    {
        /// <summary>Converts to auditinfo.</summary>
        /// <param name="audit">The audit.</param>
        /// <returns></returns>
        [CanBeNull]
        internal static Audit.AuditInfo ToAuditInfo([CanBeNull] this Audit.Audit audit)
        {
            if (null == audit)
            {
                return null;
            }


            var info = new AuditInfo()
            {
                Id            = audit.Id,
                Author        = audit.Author,
                DateTime      = audit.DateTime,
                TableName     = audit.TableName,
                TransactionId = audit.TransactionId,
                KeyValues     = audit.KeyValues
            };

            if (null != audit.OldValues && audit.OldValues.Length > 0)
            {
                info.JsonOldValues = Encoding.UTF8.GetString(audit.OldValues);
            }

            if(null != audit.NewValues && audit.NewValues.Length > 0)
            {
                info.JsonNewValues = Encoding.UTF8.GetString(audit.NewValues);
            }

            return info;
        }
    }
}