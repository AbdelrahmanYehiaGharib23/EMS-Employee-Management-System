using EMS.BLL.Models.Dto.LeaveDto;
using EMS.BLL.Services.LeaveServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.PL.Controllers
{
    [Authorize]
    public class LeaveController : Controller
    {
        private readonly ILeaveRequestService _leaveService;
        private readonly ILogger<LeaveController> _logger;

        public LeaveController(ILeaveRequestService leaveService, ILogger<LeaveController> logger)
        {
            _leaveService = leaveService;
            _logger = logger;
        }

        // Employee view of own requests. In a real app, derive employee id from User claims.
        
        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> Index(int? employeeId)
        {
            if (!employeeId.HasValue) return BadRequest("employeeId is required in this demo endpoint");
            var list = await _leaveService.GetRequestsForEmployeeAsync(employeeId.Value);
            return View(list);
        }

        [HttpGet]
        public IActionResult Create(int employeeId)
        {
            ViewBag.EmployeeId = employeeId;
            return View();
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> Create(CreateLeaveRequestDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                var res = await _leaveService.CreateLeaveRequestAsync(dto);
                if (res > 0) return RedirectToAction("Index", new { employeeId = dto.EmployeeId });
                ModelState.AddModelError(string.Empty, "Unable to create leave request.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return View(dto);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> Approvals()
        {
            var list = await _leaveService.GetPendingRequestsAsync();
            return View(list);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> GetPendingRequestsJson()
        {
            try
            {
                var list = await _leaveService.GetPendingRequestsAsync();
                return Json(list);
            }
            catch (Exception ex)
            {
                // Log server side and return diagnostic information for developer debugging
                _logger.LogError(ex, "Failed to load pending leave requests for JSON endpoint.");
                Response.StatusCode = 500;
                return Json(new { error = "Failed to load pending requests", message = ex.Message, details = ex.ToString() });
            }
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> Approve(int id, string approver)
        {
            var ok = await _leaveService.ApproveRequestAsync(id, approver);
            if (ok) return RedirectToAction("Approvals");
            return BadRequest();
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> Reject(int id, string approver)
        {
            var ok = await _leaveService.RejectRequestAsync(id, approver);
            if (ok) return RedirectToAction("Approvals");
            return BadRequest();
        }
    }
}
