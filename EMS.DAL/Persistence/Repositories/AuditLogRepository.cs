namespace EMS.DAL.Persistence.Repositories
{
    using EMS.DAL.Contracts.Repositories;
    using EMS.DAL.Entities.AuditEntity;
    using EMS.DAL.Persistence.Data.DbInitializer;
    using Microsoft.EntityFrameworkCore;

    // Repository implementation for Audit Log CRUD operations
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // Get all audit logs for a specific table
        public async Task<IEnumerable<AuditLog>> GetAuditsByTableAsync(string tableName)
        {
            return await _context.AuditLogs
                .Where(a => a.TableName == tableName)
                .OrderByDescending(a => a.ChangedDate)
                .ToListAsync();
        }

        // Get audit logs for a specific record (track history of one entity)
        public async Task<IEnumerable<AuditLog>> GetAuditsByRecordAsync(string tableName, string recordId)
        {
            return await _context.AuditLogs
                .Where(a => a.TableName == tableName && a.RecordId == recordId)
                .OrderByDescending(a => a.ChangedDate)
                .ToListAsync();
        }

        // Get all changes made by a specific user
        public async Task<IEnumerable<AuditLog>> GetAuditsByUserAsync(string userId)
        {
            return await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.ChangedDate)
                .ToListAsync();
        }

        // Get audit logs within a specific date range (for reporting)
        public async Task<IEnumerable<AuditLog>> GetAuditsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AuditLogs
                .Where(a => a.ChangedDate >= startDate && a.ChangedDate <= endDate)
                .OrderByDescending(a => a.ChangedDate)
                .ToListAsync();
        }
    }
}
