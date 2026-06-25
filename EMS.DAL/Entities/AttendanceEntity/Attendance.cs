using EMS.DAL.Entities.EmployeeEntity;
using EMS.DAL.Entities.Shared.Enums;
using System;

namespace EMS.DAL.Entities.AttendanceEntity
{
    public class Attendance : BaseEntity
    {
        public int EmployeeId { get; set; }
        public DateOnly Date { get; set; }
        public AttendanceStatus Status { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public int OvertimeMinutes { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Employee? Employee { get; set; }

        // Location tracking
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsInsideCompany { get; set; }
    }
}
