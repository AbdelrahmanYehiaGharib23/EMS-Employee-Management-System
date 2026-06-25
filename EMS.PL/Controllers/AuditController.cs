using EMS.BLL.Services.AuditService;
using EMS.DAL.Contracts.UnitOfWork;
using EMS.DAL.Entities.AuditEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.PL.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class AuditController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? tableName,
            string? action,
            string? search,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var audits = (await _unitOfWork.AuditLogRepository.GetAllAsync())
                .OrderByDescending(a => a.ChangedDate)
                .ToList();

            var filteredAudits = ApplyFilters(audits, tableName, action, search, fromDate, toDate)
                .Take(300)
                .Select(MapToDto)
                .ToList();

            PopulateViewData(audits, tableName, action, search, fromDate, toDate);
            return View(filteredAudits);
        }

        [HttpGet]
        public async Task<IActionResult> RecordHistory(string tableName, string recordId)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(recordId))
                return RedirectToAction(nameof(Index));

            var audits = (await _unitOfWork.AuditLogRepository.GetAuditsByRecordAsync(tableName, recordId))
                .OrderByDescending(a => a.ChangedDate)
                .ToList();

            PopulateViewData(audits, tableName, null, recordId, null, null);
            ViewBag.PageTitle = $"Record History: {tableName} #{recordId}";

            return View("Index", audits.Select(MapToDto).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> UserActivity(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return RedirectToAction(nameof(Index));

            var audits = (await _unitOfWork.AuditLogRepository.GetAuditsByUserAsync(userId))
                .OrderByDescending(a => a.ChangedDate)
                .ToList();

            PopulateViewData(audits, null, null, userId, null, null);
            ViewBag.PageTitle = $"User Activity: {userId}";

            return View("Index", audits.Select(MapToDto).ToList());
        }

        [HttpGet]
        public Task<IActionResult> DateRange(DateTime startDate, DateTime endDate)
        {
            return Index(null, null, null, startDate, endDate);
        }

        [HttpGet]
        public Task<IActionResult> TableLogs(string tableName)
        {
            return Index(tableName, null, null, null, null);
        }

        private static IEnumerable<AuditLog> ApplyFilters(
            IEnumerable<AuditLog> audits,
            string? tableName,
            string? action,
            string? search,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = audits;

            if (!string.IsNullOrWhiteSpace(tableName))
                query = query.Where(a => string.Equals(a.TableName, tableName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(a => string.Equals(a.Action, action, StringComparison.OrdinalIgnoreCase));

            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;
                query = query.Where(a => a.ChangedDate >= from);
            }

            if (toDate.HasValue)
            {
                var to = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.ChangedDate <= to);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    Contains(a.UserEmail, search) ||
                    Contains(a.UserId, search) ||
                    Contains(a.TableName, search) ||
                    Contains(a.RecordId, search) ||
                    Contains(a.ChangeDescription, search) ||
                    Contains(a.SourceIp, search));
            }

            return query.OrderByDescending(a => a.ChangedDate);
        }

        private void PopulateViewData(
            IEnumerable<AuditLog> audits,
            string? tableName,
            string? action,
            string? search,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var auditList = audits.ToList();

            ViewBag.TableName = tableName;
            ViewBag.Action = action;
            ViewBag.Search = search;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Tables = auditList
                .Select(a => a.TableName)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t)
                .ToList();
            ViewBag.TotalCount = auditList.Count;
            ViewBag.CreateCount = auditList.Count(a => a.Action == "Create");
            ViewBag.UpdateCount = auditList.Count(a => a.Action == "Update");
            ViewBag.DeleteCount = auditList.Count(a => a.Action == "Delete");
            ViewBag.PageTitle ??= "Audit Trail";
        }

        private static bool Contains(string? value, string search)
        {
            return !string.IsNullOrEmpty(value) &&
                   value.Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        private static AuditLogDto MapToDto(AuditLog log)
        {
            return new AuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserEmail = log.UserEmail,
                TableName = log.TableName,
                RecordId = log.RecordId,
                Action = log.Action,
                ChangedDate = log.ChangedDate,
                OldValues = log.OldValues,
                NewValues = log.NewValues,
                ChangeDescription = log.ChangeDescription,
                SourceIp = log.SourceIp
            };
        }
    }
}
