using Microsoft.EntityFrameworkCore;
using PH.UowEntityFramework.EntityFramework.Abstractions.Models;

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