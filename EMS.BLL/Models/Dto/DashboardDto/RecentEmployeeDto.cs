using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Models.Dto.Dashboard_Dto
{
    public class RecentEmployeeDto
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
