namespace EMS.BLL.Services.AuditService
{
    // Data Transfer Object for audit log responses
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime ChangedDate { get; set; }
        public string OldValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
        public string ChangeDescription { get; set; } = string.Empty;
        public string SourceIp { get; set; } = string.Empty;
    }
}
