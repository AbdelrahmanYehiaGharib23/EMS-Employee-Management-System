using EMS.DAL.Entities.Shared.Enums;
using System;

namespace EMS.BLL.Models.Dto.LeaveDto
{
    public class LeaveRequestDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
        public LeaveStatus Status { get; set; }
        public DateTime RequestedOn { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? EmployeeName { get; set; }
    }
}
