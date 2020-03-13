using System;
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

            throw new NotImplementedException();

           
        }
    }
}