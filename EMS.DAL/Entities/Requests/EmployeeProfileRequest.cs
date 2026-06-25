using EMS.DAL.Entities;
using System;

namespace EMS.DAL.Entities.Requests
{
    public class EmployeeProfileRequest : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int? Age { get; set; }
        public string? Address { get; set; }
        public decimal? Salary { get; set; }
        public bool IsMarred { get; set; }
        public DateTime? HiringTime { get; set; }
        public string? Gender { get; set; }
        public string? EmployeeType { get; set; }
        public int? DepartmentId { get; set; }
        public string? ImageName { get; set; }
        public string? Message { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
