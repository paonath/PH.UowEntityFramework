using Microsoft.EntityFrameworkCore;

namespace PH.UowEntityFramework.EntityFramework.Infrastructure
{
    internal interface IAuditContext
    {
        DbSet<TransactionAudit> TransactionAudits { get; set; }
    }
}