namespace EMS.BLL.Services.AuditService
{
    using System.Text.Json;
    using EMS.DAL.Contracts.UnitOfWork;
    using EMS.DAL.Entities.AuditEntity;

    // Implementation of audit logging service
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Get complete history of changes for a specific record
        public async Task<IEnumerable<AuditLogDto>> GetRecordHistoryAsync(string tableName, string recordId)
        {
            var audits = await _unitOfWork.AuditLogRepository.GetAuditsByRecordAsync(tableName, recordId);
            return audits.Select(MapToDto).OrderByDescending(a => a.ChangedDate);
        }

        // Get all changes made by a specific user
        public async Task<IEnumerable<AuditLogDto>> GetUserAuditsAsync(string userId)
        {
            var audits = await _unitOfWork.AuditLogRepository.GetAuditsByUserAsync(userId);
            return audits.Select(MapToDto).OrderByDescending(a => a.ChangedDate);
        }

        // Get audit logs within a date range
        public async Task<IEnumerable<AuditLogDto>> GetAuditsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var audits = await _unitOfWork.AuditLogRepository.GetAuditsByDateRangeAsync(startDate, endDate);
            return audits.Select(MapToDto).OrderByDescending(a => a.ChangedDate);
        }

        // Get all audits for a specific table
        public async Task<IEnumerable<AuditLogDto>> GetTableAuditsAsync(string tableName)
        {
            var audits = await _unitOfWork.AuditLogRepository.GetAuditsByTableAsync(tableName);
            return audits.Select(MapToDto);
        }

        // Map AuditLog entity to DTO
        private AuditLogDto MapToDto(AuditLog log)
        {
            return new AuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserEmail = log.UserEmail,
                TableName = log.TableName,
                RecordId = log.RecordId,
                Action = log.Action,
                ChangedDate = log.ChangedDate,
                OldValues = log.OldValues,
                NewValues = log.NewValues,
                ChangeDescription = log.ChangeDescription,
                SourceIp = log.SourceIp
            };
        }
    }
}
