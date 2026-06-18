using Application.Interfaces.Repositories.Audit;
using Domain.Entities.Audits;
using Infrastructure.Data.Database;

namespace Infrastructure.Repositories.Audit
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Add(AuditLog auditLog)
        {
            await _context.AuditLog.AddAsync(auditLog);
        }
    }
}
