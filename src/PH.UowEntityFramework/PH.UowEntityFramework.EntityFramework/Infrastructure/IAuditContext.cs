using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using PH.UowEntityFramework.EntityFramework.Abstractions.Models;

[assembly:InternalsVisibleTo("PH.UowEntityFramework.EntityFramework.Identity")]
namespace PH.UowEntityFramework.EntityFramework.Infrastructure
{
    internal interface IAuditContext
    {
        DbSet<TransactionAudit> TransactionAudits { get; set; }
        //DbSet<Audit.Audit> Audits { get; set; }

        //TransactionAudit TransactionAudit { get; }
        string Author { get; set; }
    }
}