namespace EMS.BLL.Models.Dto.DashboardDto
{
    public class MostPresentEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string? Name { get; set; }
        public int PresentDays { get; set; }
        public int LateCount { get; set; }
        public int TotalOvertimeMinutes { get; set; }
    }
}
