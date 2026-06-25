using EMS.BLL.Services.DashboardServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EMS.PL.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IConfiguration _configuration;

        public AttendanceController(IDashboardService dashboardService, IConfiguration configuration)
        {
            _dashboardService = dashboardService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Title = "Attendance";
            ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
            return View();
        }

        public class GeoDto
        {
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> CheckIn([FromBody] GeoDto geo)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var lat = geo?.Latitude ?? 0.0;
            var lng = geo?.Longitude ?? 0.0;

            var ok = await _dashboardService.CheckInByEmailAsync(email, lat, lng);

            if (!ok)
                return BadRequest(new { message = "Check-in failed. Verify company location or existing check-in." });

            return Ok(new { message = "Checked in successfully." });
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> CheckOut()
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var ok = await _dashboardService.CheckOutByEmailAsync(email, null);
            if (!ok)
                return BadRequest(new { message = "Check-out failed or already completed." });

            return Ok(new { message = "Checked out successfully." });
        }

        [HttpGet]
        public IActionResult WhoAmI()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Json(new { name = User.Identity?.Name, claims });
        }
    }
}
