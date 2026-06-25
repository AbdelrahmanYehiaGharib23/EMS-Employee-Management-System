using EMS.BLL.Models.Dto.Dashboard_Dto;
using EMS.BLL.Models.Dto.DashboardDto.DashboardAnalyticsDto;
using EMS.BLL.Models.Dto.DashboardDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Services.DashboardServices
{
    public interface IDashboardService
    {
        System.Threading.Tasks.Task<DashboardStatsDto> GetStatsAsync();
        System.Threading.Tasks.Task<DashboardAnalyticsDto> GetAnalyticsAsync();
        System.Threading.Tasks.Task<List<HiringTrendDto>> GetHiringTrendAsync();
        System.Threading.Tasks.Task<byte[]> GenerateAnalyticsReportCsvAsync();
        System.Threading.Tasks.Task<List<MostPresentEmployeeDto>> GetMostPresentEmployeesAsync(int top = 5, int days = 30);
        System.Threading.Tasks.Task<bool> CheckInByEmailAsync(string userEmail,double latitude,double longitude,DateTime? timestamp = null);
        System.Threading.Tasks.Task<bool> CheckOutByEmailAsync(string userEmail, DateTime? timestamp = null);

    }
}
