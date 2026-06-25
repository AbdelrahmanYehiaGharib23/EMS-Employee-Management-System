using System;
using System.Collections.Generic;

namespace EMS.BLL.Models.Dto.EmployeeDto
{
    public class DailyHourDto
    {
        public DateTime Date { get; set; }
        public decimal Hours { get; set; }
    }

    public class EmployeeDashboardDto
    {
        // 0-100
        public int ProductivityScore { get; set; }

        // Behavior flags like "Frequent Late", "High Absence", "High Overtime"
        public List<string> BehaviorFlags { get; set; } = new List<string>();

        // Last 7 days hours
        public List<DailyHourDto> DailyHours { get; set; } = new List<DailyHourDto>();

        // Summary: total hours in period (e.g., last 7 or 30 days)
        public decimal TotalHoursLast30Days { get; set; }

        // Simple recommendations
        public List<string> Recommendations { get; set; } = new List<string>();
    }
}
