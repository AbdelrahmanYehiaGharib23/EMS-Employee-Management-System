using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Models.Dto.DashboardDto.DashboardAnalyticsDto
{
    public class DepartmentChartDto
    {
        public string Department { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
