using EMS.DAL.Entities.EmployeeEntity;
using EMS.DAL.Entities.Shared.Enums;
using System;

namespace EMS.DAL.Entities.LeaveEntity
{
    public class LeaveRequest : BaseEntity
    {
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
        public LeaveStatus Status { get; set; }
        public DateTime RequestedOn { get; set; } = DateTime.UtcNow;
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }

        // Navigation
        public virtual Employee? Employee { get; set; }
    }
}
