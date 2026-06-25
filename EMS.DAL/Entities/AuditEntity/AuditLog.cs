namespace EMS.DAL.Entities.AuditEntity
{
    // Entity to store audit trail records for tracking all changes in the system
    public class AuditLog : BaseEntity
    {
        // User who made the change
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        // Table/Entity that was changed
        public string TableName { get; set; } = string.Empty;

        // Primary key value of the record that was changed
        public string RecordId { get; set; } = string.Empty;

        // Type of change: Create, Update, Delete
        public string Action { get; set; } = string.Empty;

        // Date and time of the change
        public DateTime ChangedDate { get; set; }

        // Old values in JSON format (null for Create)
        public string OldValues { get; set; } = string.Empty;

        // New values in JSON format (null for Delete)
        public string NewValues { get; set; } = string.Empty;

        // Description of what changed
        public string ChangeDescription { get; set; } = string.Empty;

        // IP Address or source of the change
        public string SourceIp { get; set; } = string.Empty;
    }
}
