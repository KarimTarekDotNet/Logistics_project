using Domain.Entities.Audits;

namespace Application.Interfaces.Repositories.Audit
{
    public interface IAuditLogRepository
    {
        Task Add(AuditLog auditLog);
    }
}
