using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Models.Dto.Dashboard_Dto
{
    public class DepartmentEmployeeCountDto
    {

        public string DepartmentName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
    }
}
