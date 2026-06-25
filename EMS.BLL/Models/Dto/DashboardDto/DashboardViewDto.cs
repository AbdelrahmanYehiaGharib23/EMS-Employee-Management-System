using EMS.BLL.Models.Dto.Dashboard_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Models.Dto.DashboardDto.DashboardAnalyticsDto
{
    public class DashboardViewDto
    {
        public DashboardStatsDto Stats { get; set; } = new();
        public DashboardAnalyticsDto Analytics { get; set; } = new();
    }
}
