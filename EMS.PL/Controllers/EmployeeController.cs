using AutoMapper;
using EMS.BLL.Models.Dto.DepartmentDto;
using EMS.BLL.Models.Dto.EmployeeDto;
using EMS.BLL.Services.DepartmentServices;
using EMS.BLL.Services.EmployeeServices;
using EMS.DAL.Entities.EmployeeEntity;
using EMS.DAL.Entities.Shared.Enums;
using EMS.PL.ViewModels.Department;
using EMS.PL.ViewModels.Employee;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EMS.DAL.Entities.IdentityModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EMS.PL.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EMS.BLL.Services.Notification.INotificationService _notificationService;
        private readonly EMS.BLL.Services.Identity.IEmailSender _emailSender;
        private readonly EMS.DAL.Contracts.UnitOfWork.IUnitOfWork _unitOfWork;

        public EmployeeController(
            IEmployeeService employeeService, ILogger<EmployeeController> logger, IWebHostEnvironment environment, IMapper mapper,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, 
            EMS.DAL.Contracts.UnitOfWork.IUnitOfWork unitOfWork,
            EMS.BLL.Services.Notification.INotificationService notificationService,
            EMS.BLL.Services.Identity.IEmailSender emailSender)
        {
            _employeeService = employeeService;
            _logger = logger;
            _environment = environment;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> Index(string? employeeSearchByName)
        {
            var employees = await _employeeService.GetEmployeesAsync(employeeSearchByName);
            return View(employees);
        }

        [HttpGet]
        [Authorize]
        public async System.Threading.Tasks.Task<IActionResult> Profile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Challenge();

            try
            {
                var employee = await _employeeService.GetEmployeeByEmailAsync(email);

                if (employee == null)
                    return View("NoEmployeeRecord");

                var dashboard = await _employeeService.GetEmployeeDashboardAsync(employee.Id);

                var vm = new EMS.PL.ViewModels.Employee.EmployeeProfileViewModel
                {
                    Employee = employee,
                    Dashboard = dashboard,
                    CompanyLocationName = (await _unitOfWork.CompanyLocationRepository.GetAllAsync()).FirstOrDefault()?.Name
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee profile");
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectEmployeeRequest(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest();

            try
            {
                var req = (await _unitOfWork.EmployeeProfileRequestRepository.GetAllAsync())
                    .FirstOrDefault(r => r.Email == email && r.Status == "Pending");
                    
                if (req != null)
                {
                    req.Status = "Rejected";
                    _unitOfWork.EmployeeProfileRequestRepository.Update(req);
                    await _unitOfWork.CompleteAsync();
                }

                var payload = new { Action = "ProfileRejected", Email = email };
                await _notificationService.NotifyUserAsync(email, "Profile Request Rejected", "Your profile request was rejected.", payload);
                await _notificationService.NotifyRoleAsync("HR", "Profile Request Rejected", $"Request for {email} was rejected.", payload);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting employee request for {Email}", email);
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestEmployeeRecord()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.Identity?.Name ?? email;

            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Unable to determine your email. Please re-login.";
                return View("NoEmployeeRecord");
            }

            var title = "Employee Profile Request";
            var message = $"{name} ({email}) has requested an employee profile.";
            var payload = new { RequestType = "EmployeeProfile", Name = name, Email = email, RequestedAtUtc = DateTime.UtcNow };

            try
            {
                await _notificationService.NotifyRoleAsync("HR", title, message, payload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SignalR notification failed");
            }

            try
            {
                var req = new EMS.DAL.Entities.Requests.EmployeeProfileRequest
                {
                    Name = name ?? string.Empty,
                    Email = email,
                    Message = "Requested via UI",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.EmployeeProfileRequestRepository.Add(req);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save profile request");
            }

            TempData["SuccessMessage"] = "Your request has been submitted to HR.";
            return View("NoEmployeeRecord");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveEmployeeRequest(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { success = false, message = "Email is required" });

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Forbid();

            try
            {
                var req = (await _unitOfWork.EmployeeProfileRequestRepository.GetAllAsync())
                    .FirstOrDefault(r => r.Email == email && r.Status == "Pending");

                if (req == null)
                    return NotFound(new { success = false, message = "No pending request found" });

                var targetUser = await _userManager.FindByEmailAsync(email);

                var createDto = new CreateEmployeeDto
                {
                    Name = !string.IsNullOrWhiteSpace(req.Name) ? req.Name : targetUser?.UserName ?? email,
                    Email = email,
                    PhoneNumber = req.PhoneNumber ?? targetUser?.PhoneNumber,
                    Age = req.Age,
                    Address = req.Address ?? "",
                    HiringTime = req.HiringTime.HasValue ? DateOnly.FromDateTime(req.HiringTime.Value) : DateOnly.FromDateTime(DateTime.UtcNow),
                    Salary = req.Salary ?? 0m,
                    IsMarred = req.IsMarred,
                    DepartmentId = req.DepartmentId,
                    ImageName = req.ImageName,
                    Gender = Enum.TryParse(req.Gender, true, out Gender g) ? g : Gender.male,
                    EmployeeType = Enum.TryParse(req.EmployeeType, true, out EmployeeType et) ? et : EmployeeType.FullTime
                };

                var createdId = await _employeeService.CreateEmployeeAsync(createDto);

                req.Status = createdId > 0 ? "Completed" : "Approved";
                _unitOfWork.EmployeeProfileRequestRepository.Update(req);
                await _unitOfWork.CompleteAsync();

                if (targetUser != null && createdId > 0)
                {
                    const string roleName = "Employee";
                    if (!await _roleManager.RoleExistsAsync(roleName))
                        await _roleManager.CreateAsync(new IdentityRole(roleName));

                    if (!await _userManager.IsInRoleAsync(targetUser, roleName))
                        await _userManager.AddToRoleAsync(targetUser, roleName);
                }

                await _notificationService.NotifyUserAsync(email, "Profile Approved", "Your employee profile has been created.", 
                    new { Action = "ProfileApproved", EmployeeId = createdId, Email = email });

                return Ok(new { success = true, created = createdId > 0, employeeId = createdId, message = "Request processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving request for {Email}", email);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult Create(string? email = null)
        {
            var model = new EmployeeViewModel { HiringTime = DateOnly.FromDateTime(DateTime.UtcNow) };
            
            if (!string.IsNullOrEmpty(email))
            {
                model.Email = email;
                try
                {
                    var appUser = _userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
                    if (appUser != null)
                    {
                        model.Name = string.IsNullOrEmpty(appUser.FirstName) ? appUser.UserName :
                            (appUser.FirstName + (string.IsNullOrEmpty(appUser.LastName) ? "" : " " + appUser.LastName));
                        model.PhoneNumber = appUser.PhoneNumber;
                    }
                }
                catch { }
            }
            
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> Create(EmployeeViewModel employeeViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(employeeViewModel.Email))
                    {
                        var existing = await _employeeService.GetEmployeeByEmailAsync(employeeViewModel.Email);
                        if (existing != null)
                        {
                            ModelState.AddModelError(string.Empty, "An employee record already exists for this email");
                            return View(employeeViewModel);
                        }
                    }

                    var employeeDto = new CreateEmployeeDto()
                    {
                        Name = employeeViewModel.Name,
                        Email = employeeViewModel.Email,
                        PhoneNumber = employeeViewModel.PhoneNumber,
                        Age = employeeViewModel.Age,
                        Address = employeeViewModel.Address,
                        Salary = employeeViewModel.Salary,
                        IsMarred = employeeViewModel.IsMarred,
                        HiringTime = employeeViewModel.HiringTime,
                        Gender = employeeViewModel.Gender,
                        EmployeeType = employeeViewModel.EmployeeType,
                        DepartmentId = employeeViewModel.DepartmentId,
                        Image = employeeViewModel.Image,
                    };

                    int result = await _employeeService.CreateEmployeeAsync(employeeDto);
                    if (result > 0)
                    {
                        try
                        {
                            var req = _unitOfWork.EmployeeProfileRequestRepository.GetAll()
                                .FirstOrDefault(r => r.Email == employeeViewModel.Email && (r.Status == "Pending" || r.Status == "Approved"));
                            if (req != null)
                            {
                                req.Status = "Completed";
                                _unitOfWork.EmployeeProfileRequestRepository.Update(req);
                                await _unitOfWork.CompleteAsync();
                            }
                        }
                        catch { }

                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(string.Empty, "Failed to create employee");
                }
                catch (Exception ex)
                {
                    if (_environment.IsDevelopment())
                        ModelState.AddModelError(string.Empty, ex.Message);
                    else
                        _logger.LogError(ex.Message);
                }
            }

            return View(employeeViewModel);
        }

        [HttpGet]
        [Authorize]
        public async System.Threading.Tasks.Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue) return BadRequest();
            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee is null) return NotFound();
            return View(employee);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async System.Threading.Tasks.Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue) return BadRequest();
            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee is null) return NotFound();

            var viewModel = _mapper.Map<EmployeeViewModel>(employee);
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> Edit(int id, EmployeeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var updateDto = _mapper.Map<UpdateEmployeeDto>(viewModel);
                    updateDto.Id = id;

                    if (viewModel.Image is not null)
                    {
                        var result = await _employeeService.UpdateEmployeeAsync(updateDto);
                        if (result > 0) return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        var result = await _employeeService.UpdateEmployeeAsync(updateDto);
                        if (result > 0) return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(string.Empty, "Failed to update employee");
                }
                catch (Exception ex)
                {
                    if (_environment.IsDevelopment())
                        ModelState.AddModelError(string.Empty, ex.Message);
                    else
                        _logger.LogError(ex.Message);
                }
            }

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async System.Threading.Tasks.Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue) return BadRequest();
            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee is null) return NotFound();
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id == 0) return BadRequest();

            try
            {
                bool deleted = await _employeeService.DeleteEmployeeAsync(id);
                if (deleted)
                {
                    _logger.LogInformation($"Employee deleted: ID={id}");
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "Failed to delete employee");
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee");
                return View("ErrorView", ex);
            }
        }
    }
}
