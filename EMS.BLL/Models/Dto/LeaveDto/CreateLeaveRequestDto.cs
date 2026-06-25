using System;

namespace EMS.BLL.Models.Dto.LeaveDto
{
    public class CreateLeaveRequestDto
    {
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
    }
}
