using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Models.Dto.Dashboard_Dto
{
    public class DashboardStatsDto
    {
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int NewEmployeesLast30Days { get; set; }

        public List<DepartmentEmployeeCountDto> EmployeesPerDepartment { get; set; } = new();
        public List<RecentEmployeeDto> RecentEmployees { get; set; } = new();

    }



}
