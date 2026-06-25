using Microsoft.AspNetCore.Mvc;
using EMS.DAL.Persistence.Data.DbInitializer;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using EMS.BLL.Services.Notification;

namespace EMS.PL.Controllers
{
    // lightweight debugging endpoints to inspect DB schema and force EnsureCreated in dev
    [Authorize(Roles = "Admin")]
    public class DebugController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DebugController> _logger;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _environment;

        private readonly IConfiguration _config;

        public DebugController(ApplicationDbContext db, ILogger<DebugController> logger, IConfiguration config,INotificationService notificationService, IWebHostEnvironment environment)
        {
            _db = db;
            _logger = logger;
            _config = config;
            _notificationService = notificationService;
            _environment = environment;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTestNotification(string role = "HR", string? email = null)
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            try
            {
                var title = "Test Notification";
                var message = "This is a test notification from DebugController.";
                var payload = new { From = "debug@test", Email = email };
                if (!string.IsNullOrEmpty(role))
                {
                    await _notificationService.NotifyRoleAsync(role, title, message, payload);
                }
                if (!string.IsNullOrEmpty(email))
                {
                    await _notificationService.NotifyUserAsync(email, title, message, payload);
                }
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendTestNotification failed");
                Response.StatusCode = 500;
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Tables()
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            try
            {
                var tables = new List<string>();
                // Use ADO.NET via the configured connection string to enumerate tables
                var connString = _config.GetConnectionString("DefaultConnection") ?? _config.GetConnectionString("MyConnection") ?? string.Empty;
                if (string.IsNullOrEmpty(connString))
                    throw new InvalidOperationException("No connection string found in configuration (DefaultConnection/MyConnection)");

                using (var conn = new Microsoft.Data.SqlClient.SqlConnection(connString))
                {
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT name FROM sys.tables ORDER BY name";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        tables.Add(reader.GetString(0));
                    }
                }
                return Json(new { ok = true, tables });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enumerate tables");
                Response.StatusCode = 500;
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnsureCreated()
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            try
            {
                var created = _db.Database.EnsureCreated();
                return Json(new { ok = true, ensured = created });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EnsureCreated failed");
                Response.StatusCode = 500;
                return Json(new { ok = false, error = ex.Message });
            }
        }
    }
}
