namespace EMS.BLL.Services.AuditService
{
    using System.Text.Json;
    using EMS.DAL.Entities.AuditEntity;

    // Interface for audit logging operations
    public interface IAuditService
    {
        // Get audit history for a specific record
        Task<IEnumerable<AuditLogDto>> GetRecordHistoryAsync(string tableName, string recordId);

        // Get all changes made by a user
        Task<IEnumerable<AuditLogDto>> GetUserAuditsAsync(string userId);

        // Get changes within a date range
        Task<IEnumerable<AuditLogDto>> GetAuditsByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Get all audits for a table
        Task<IEnumerable<AuditLogDto>> GetTableAuditsAsync(string tableName);
    }
}
