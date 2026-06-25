using EMS.BLL.Models.Dto.DashboardDto.DashboardAnalyticsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Models.Dto.DashboardDto.DashboardAnalyticsDto
{
    public class DashboardAnalyticsDto
    {

        public List<DepartmentChartDto> DepartmentChart { get; set; } = new();
        public List<HiringTrendDto> HiringTrend { get; set; } = new();
        public GenderDistributionDto GenderDistribution { get; set; } = new();
        public SalaryAnalyticsDto SalaryAnalytics { get; set; } = new();


    }
}
