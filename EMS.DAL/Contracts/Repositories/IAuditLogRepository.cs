namespace EMS.DAL.Contracts.Repositories
{
    using EMS.DAL.Entities.AuditEntity;

    // Repository interface for Audit Log operations
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        // Get audit logs for a specific entity by table name
        Task<IEnumerable<AuditLog>> GetAuditsByTableAsync(string tableName);

        // Get audit logs for a specific record
        Task<IEnumerable<AuditLog>> GetAuditsByRecordAsync(string tableName, string recordId);

        // Get audit logs for a specific user
        Task<IEnumerable<AuditLog>> GetAuditsByUserAsync(string userId);

        // Get audit logs within a date range
        Task<IEnumerable<AuditLog>> GetAuditsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
