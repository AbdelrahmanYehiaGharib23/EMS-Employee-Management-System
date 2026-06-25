using EMS.BLL.Models.Dto.EmployeeDto;

namespace EMS.PL.ViewModels.Employee
{
    public class EmployeeProfileViewModel
    {
        public EmployeeDetailsDto Employee { get; set; } = null!;
        public EmployeeDashboardDto Dashboard { get; set; } = null!;
        // Company location name to show on employee profile
        public string? CompanyLocationName { get; set; }
    }
}
