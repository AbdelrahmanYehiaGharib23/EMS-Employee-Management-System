using EMS.BLL.Models.Dto.DashboardDto.DashboardAnalyticsDto;
using EMS.BLL.Services.DashboardServices;
using EMS.DAL.Entities.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.PL.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly EMS.DAL.Contracts.UnitOfWork.IUnitOfWork _unitOfWork;

        public DashboardController(IDashboardService dashboardService, EMS.DAL.Contracts.UnitOfWork.IUnitOfWork unitOfWork)
        {
            _dashboardService = dashboardService;
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles = "Admin,HR")]
        public async System.Threading.Tasks.Task<IActionResult> Index()
        {
            var profileRequests = (await _unitOfWork.EmployeeProfileRequestRepository.GetAllAsync()).ToList();
            var leaveRequests = (await _unitOfWork.LeaveRequestRepository.GetAllAsync()).ToList();

            ViewBag.IsHR = User.IsInRole("HR");
            ViewBag.PendingProfileRequests = profileRequests.Count(r => string.Equals(r.Status, "Pending", StringComparison.OrdinalIgnoreCase));
            ViewBag.CompletedProfileRequests = profileRequests.Count(r => string.Equals(r.Status, "Completed", StringComparison.OrdinalIgnoreCase));
            ViewBag.PendingLeaveRequests = leaveRequests.Count(r => r.Status == LeaveStatus.Pending);
            ViewBag.RecentProfileRequests = profileRequests
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList();

            var model = new DashboardViewDto
            {
                Stats = await _dashboardService.GetStatsAsync(),
                Analytics = await _dashboardService.GetAnalyticsAsync()
            };
            ViewBag.HiringTrend = await _dashboardService.GetHiringTrendAsync();
            return View(model);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> GetCompanyLocation()
        {
            var location = (await _unitOfWork.CompanyLocationRepository.GetAllAsync()).FirstOrDefault();
            if (location == null) return Json(null);
            return Json(new
            {
                name = location.Name,
                latitude = location.Latitude,
                longitude = location.Longitude,
                radius = location.AllowedRadiusMeters
            });
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> GetStats()
        {
            var stats = await _dashboardService.GetStatsAsync();
            return Json(stats);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> GetAnalytics()
        {
            var analytics = await _dashboardService.GetAnalyticsAsync();
            return Json(analytics);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> GetHiringTrend()
        {
            var hiring = await _dashboardService.GetHiringTrendAsync();
            return Json(hiring);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> GetMostPresentEmployees(int top = 5, int days = 30)
        {
            var list = await _dashboardService.GetMostPresentEmployeesAsync(top, days);
            return Json(list);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> DownloadAnalyticsReport()
        {
            var csv = await _dashboardService.GenerateAnalyticsReportCsvAsync();
            return File(csv, "text/csv", "dashboard-analytics.csv");
        }
    }
}

