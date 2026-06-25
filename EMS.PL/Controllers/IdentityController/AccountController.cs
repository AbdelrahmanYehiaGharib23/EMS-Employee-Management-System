using EMS.BLL.Models.Dto.IdentityDto;
using EMS.BLL.Services.Identity;
using EMS.DAL.Entities.IdentityModel;
using EMS.PL.ViewModels.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using EMS.BLL.Services.EmployeeServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EMS.BLL.Models.Dto.EmployeeDto;
using EMS.DAL.Entities.Requests;
using EMS.DAL.Persistence.Data.DbInitializer;
using EMS.BLL.Services.AttachementService;

namespace EMS.PL.Controllers.IdentityController
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPasswordResetManager _passwordResetManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmployeeService _employeeService;
        private readonly EMS.DAL.Contracts.UnitOfWork.IUnitOfWork _unitOfWork;
        private readonly EMS.BLL.Services.Notification.INotificationService _notificationService;
        private readonly EMS.BLL.Services.Identity.IEmailSender _emailSender;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAttachementService _attachementService;
        private readonly IWebHostEnvironment _environment;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IPasswordResetManager passwordResetManager,
            RoleManager<IdentityRole> roleManager,
            IEmployeeService employeeService,
            EMS.DAL.Contracts.UnitOfWork.IUnitOfWork unitOfWork,
            EMS.BLL.Services.Notification.INotificationService notificationService,
            EMS.BLL.Services.Identity.IEmailSender emailSender,
            ApplicationDbContext dbContext,
            ILogger<AccountController> logger,
            IConfiguration configuration,
            IAttachementService attachementService,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _passwordResetManager = passwordResetManager;
            _roleManager = roleManager;
            _employeeService = employeeService;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _emailSender = emailSender;
            _dbContext = dbContext;
            _logger = logger;
            _configuration = configuration;
            _attachementService = attachementService;
            _environment = environment;
        }

        // Debug: send a manual notification to HR (used to verify SignalR delivery)
        [HttpPost]
        [Route("DebugNotify")]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DebugNotify([FromForm] string title, [FromForm] string message)
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            try
            {
                var payload = new { Debug = true, SentAt = DateTime.UtcNow };
                await _notificationService.NotifyRoleAsync("HR", title ?? "Debug Notification", message ?? string.Empty, payload);
                _logger.LogInformation("DebugNotify: sent notification to HR: {Title}", title);
                return Ok(new { sent = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DebugNotify failed to send notification to HR");
                return StatusCode(500, new { sent = false, error = ex.Message });
            }
        }
        #region Register
        //Register
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            var appUser = new ApplicationUser()
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                UserName = viewModel.UserName,
                Email = viewModel.Email,
            };

            var result = await _userManager.CreateAsync(appUser, viewModel.Password);
            if (result.Succeeded)
            {
                try
                {
                    var name = string.IsNullOrEmpty(appUser.FirstName) ? appUser.UserName :
                        (appUser.FirstName + (string.IsNullOrEmpty(appUser.LastName) ? "" : " " + appUser.LastName));
                    var req = new EmployeeProfileRequest
                    {
                        Name = name ?? string.Empty,
                        Email = appUser.Email ?? string.Empty,
                        Message = "Requested during registration",
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };

                    _unitOfWork.EmployeeProfileRequestRepository.Add(req);
                    await _unitOfWork.CompleteAsync();

                    var title = "Employee Profile Request";
                    var message = $"{name} ({appUser.Email}) has requested an employee profile.";
                    var payload = new
                    {
                        RequestType = "EmployeeProfile",
                        RequestedByName = name,
                        RequestedByEmail = appUser.Email,
                        Name = name,
                        Email = appUser.Email,
                        RequestedAtUtc = DateTime.UtcNow
                    };

                    try { await _notificationService.NotifyRoleAsync("HR", title, message, payload); }
                    catch { }
                }
                catch { }

                TempData["SuccessMessage"] = "Account created. A profile request has been sent to HR.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(viewModel);
        }
        #endregion

        // Show form for creating an employee profile request (with ability to check for existing employee by email)
        [HttpGet]
        public IActionResult ProfileRequest(string? email = null)
        {
            var vm = new ProfileRequestViewModel();
            // prefer explicit query param, otherwise try to infer from current user
            if (string.IsNullOrEmpty(email))
            {
                try { email = User?.FindFirst(ClaimTypes.Email)?.Value; } catch { email = null; }
            }

            if (!string.IsNullOrEmpty(email)) vm.Email = email;

            try
            {
                var name = User?.Identity?.Name;
                if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(vm.Name)) vm.Name = name;
            }
            catch { }

            // perform a silent employee lookup and expose result to the view so the user
            // is not required to enter their email to check. If an employee exists for
            // this email we will inform the user and prevent creating a duplicate request.
            try
            {
                if (!string.IsNullOrEmpty(vm.Email))
                {
                    var emp = _employeeService.GetEmployeeByEmailAsync(vm.Email).GetAwaiter().GetResult();
                    if (emp != null)
                    {
                        // pass the found employee to the view
                        ViewBag.EmployeeMatch = emp;
                    }
                }
            }
            catch
            {
                // ignore lookup errors — don't block the user
            }

            return View(vm);
        }

        // AJAX: check for existing employee by email and return a partial view with employee details (or empty)
        [HttpPost]
        public async Task<IActionResult> CheckEmployeeByEmail([FromForm] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return PartialView("_EmployeeMatchPartial", null);

            EmployeeDetailsDto? emp = null;
            try
            {
                emp = await _employeeService.GetEmployeeByEmailAsync(email);
            }
            catch
            {
                // ignore lookup errors and return empty partial
            }

            return PartialView("_EmployeeMatchPartial", emp);
        }

        // Submit a profile request (from the form). If an employee exists the HR will see the matched data in details.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitProfileRequest(ProfileRequestViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("ProfileRequest", vm);

            try
            {
                // if an employee already exists for this email, refuse to create a duplicate request
                try
                {
                    var existingEmployee = await _employeeService.GetEmployeeByEmailAsync(vm.Email);
                    if (existingEmployee != null)
                    {
                        TempData["ErrorMessage"] = "An employee record already exists for this email.";
                        return RedirectToAction("ProfileRequest");
                    }
                }
                catch { }

                var existingReq = _unitOfWork.EmployeeProfileRequestRepository.GetAll()
                    .FirstOrDefault(r => r.Email == vm.Email && r.Status == "Pending");
                if (existingReq != null)
                {
                    ParseHiringDate(vm.HiringTimeString, out var parsedHiring);
                    existingReq.Name = vm.Name ?? existingReq.Name;
                    existingReq.Email = vm.Email ?? existingReq.Email;
                    existingReq.PhoneNumber = vm.PhoneNumber;
                    existingReq.Age = vm.Age;
                    existingReq.Address = vm.Address;
                    existingReq.Salary = vm.Salary;
                    existingReq.IsMarred = vm.IsMarred;
                    existingReq.HiringTime = parsedHiring.HasValue
                        ? DateTime.SpecifyKind(parsedHiring.Value.ToDateTime(new TimeOnly(0, 0)), DateTimeKind.Utc)
                        : existingReq.HiringTime;
                    existingReq.Gender = vm.Gender;
                    existingReq.EmployeeType = vm.EmployeeType;
                    existingReq.DepartmentId = vm.DepartmentId;
                    if (vm.Image is not null)
                        existingReq.ImageName = await _attachementService.UploadAsync(vm.Image, "Images");
                    existingReq.Message = vm.Message;

                    _unitOfWork.EmployeeProfileRequestRepository.Update(existingReq);
                    await _unitOfWork.CompleteAsync();

                    try
                    {
                        var title = "Employee Profile Request Updated";
                        var message = $"{existingReq.Name} ({existingReq.Email}) updated their profile request.";
                        await _notificationService.NotifyRoleAsync("HR", title, message, existingReq);
                        TempData["SuccessMessage"] = "Your pending request was updated and HR has been notified.";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to notify HR about updated request");
                        TempData["SuccessMessage"] = "Your pending request was updated.";
                    }

                    return RedirectToAction("ProfileRequest");
                }

                ParseHiringDate(vm.HiringTimeString, out var hiring);

                var req = new EmployeeProfileRequest
                {
                    Name = vm.Name ?? string.Empty,
                    Email = vm.Email ?? string.Empty,
                    PhoneNumber = vm.PhoneNumber,
                    Age = vm.Age,
                    Address = vm.Address,
                    Salary = vm.Salary,
                    IsMarred = vm.IsMarred,
                    HiringTime = hiring.HasValue ? DateTime.SpecifyKind(hiring.Value.ToDateTime(new TimeOnly(0, 0)), DateTimeKind.Utc) : null,
                    Gender = vm.Gender,
                    EmployeeType = vm.EmployeeType,
                    DepartmentId = vm.DepartmentId,
                    ImageName = vm.Image is not null ? await _attachementService.UploadAsync(vm.Image, "Images") : null,
                    Message = vm.Message,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                using (var tx = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _unitOfWork.EmployeeProfileRequestRepository.Add(req);
                        var changes = await _unitOfWork.CompleteAsync();
                        if (changes <= 0)
                        {
                            await tx.RollbackAsync();
                            TempData["ErrorMessage"] = "Failed to save the request. Please try again.";
                            return RedirectToAction("ProfileRequest");
                        }

                        await tx.CommitAsync();

                        var title = "Employee Profile Request";
                        var message = $"{req.Name} ({req.Email}) has requested an employee profile.";

                        try
                        {
                            await _notificationService.NotifyRoleAsync("HR", title, message, req);
                            TempData["SuccessMessage"] = "Request submitted successfully. HR has been notified.";
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to notify HR for request {Email}", req.Email);
                            await SendEmailFallbackAsync(req);
                        }

                        return RedirectToAction("ProfileRequest");
                    }
                    catch (Exception dbEx)
                    {
                        try { await tx.RollbackAsync(); } catch { }
                        _logger.LogError(dbEx, "Failed saving profile request for {Email}", vm.Email);
                        TempData["ErrorMessage"] = "An error occurred. Please try again later.";
                        return RedirectToAction("ProfileRequest");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("ProfileRequest", vm);
            }
        }

        // Utility endpoint for client-side scripts to confirm role membership for the current user
        [HttpGet]
        public async Task<IActionResult> IsInRole(string role)
        {
            if (string.IsNullOrEmpty(role) || User?.Identity?.IsAuthenticated != true)
                return Json(new { isInRole = false });

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Json(new { isInRole = false });

            var appUser = await _userManager.FindByEmailAsync(email);
            if (appUser == null)
                return Json(new { isInRole = false });

            var roles = await _userManager.GetRolesAsync(appUser);
            var isIn = roles != null && roles.Contains(role);
            return Json(new { isInRole = isIn });
        }

        // Access Denied handler - used by Authorization when user lacks required role
        [HttpGet]
        public async Task<IActionResult> AccessDenied(string? returnUrl = null)
        {
            // If the user is authenticated attempt to resolve email and roles for diagnostics
            if (User?.Identity?.IsAuthenticated == true)
            {
                // get email claim only (do not fallback to name)
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                ViewBag.Email = email;
                ViewBag.UserName = User.Identity?.Name;

                if (!string.IsNullOrEmpty(email))
                {
                    var appUser = await _userManager.FindByEmailAsync(email);
                    if (appUser != null)
                    {
                        var roles = await _userManager.GetRolesAsync(appUser);
                        ViewBag.Roles = roles;
                        if (roles != null && roles.Contains("Employee"))
                        {
                            return RedirectToAction("Profile", "Employee");
                        }
                    }
                }
                else
                {
                    ViewBag.Roles = new string[] { };
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        #region Login
        //Login
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel viewModel, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var user = await _userManager.FindByEmailAsync(viewModel.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(viewModel);
            }

            var result = await _signInManager.PasswordSignInAsync(user, viewModel.Password, viewModel.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // If there is an employee record for this email, ensure the user has Employee role and refresh sign-in
                try
                {
                    var userEmail = user.Email;
                    if (!string.IsNullOrWhiteSpace(userEmail))
                    {
                        var emp = await _employeeService.GetEmployeeByEmailAsync(userEmail);
                        if (emp != null)
                        {
                            if (!await _roleManager.RoleExistsAsync("Employee"))
                            {
                                await _roleManager.CreateAsync(new IdentityRole("Employee"));
                            }
                            var curRoles = await _userManager.GetRolesAsync(user);
                            if (!curRoles.Contains("Employee"))
                            {
                                await _userManager.AddToRoleAsync(user, "Employee");
                                await _signInManager.RefreshSignInAsync(user);
                            }
                        }
                        else
                        {
                            // No employee record exists for this user. Ensure there is a pending profile request and notify HR.
                            try
                            {
                                var existingReq = _unitOfWork.EmployeeProfileRequestRepository.GetAll()
                                    .FirstOrDefault(r => r.Email == userEmail && (r.Status == "Pending" || r.Status == "Approved" || r.Status == "Completed"));
                                if (existingReq == null)
                                {
                                    var name = string.IsNullOrEmpty(user.FirstName) ? user.UserName :
                                        (user.FirstName + (string.IsNullOrEmpty(user.LastName) ? "" : " " + user.LastName));
                                    var req = new EmployeeProfileRequest
                                    {
                                        Name = name ?? string.Empty,
                                        Email = userEmail,
                                        Message = "Requested on login",
                                        Status = "Pending",
                                        CreatedAt = DateTime.UtcNow
                                    };
                                    _unitOfWork.EmployeeProfileRequestRepository.Add(req);
                                    await _unitOfWork.CompleteAsync();

                                    var title = "Employee Profile Request";
                                    var message = $"{name} ({userEmail}) has requested an employee profile.";
                                    try { await _notificationService.NotifyRoleAsync("HR", title, message, new { name, Email = userEmail }); }
                                    catch { }
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch
                {
                    // ignore errors from employee lookup
                }

                // re-evaluate roles after any potential refresh
                var roles = await _userManager.GetRolesAsync(user);

                // if a returnUrl was provided ensure it's local
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    // prevent redirecting Employee users to Admin-only pages (e.g. Dashboard)
                    if (roles != null && roles.Any() && roles.Contains("Employee") && returnUrl.IndexOf("/Dashboard", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // ignore returnUrl for employees and send to their profile
                        return RedirectToAction("Profile", "Employee");
                    }

                    return Redirect(returnUrl);
                }

                if (roles != null && roles.Any())
                {
                    if (roles.Contains("Admin") || roles.Contains("HR"))
                        return RedirectToAction("Index", "Dashboard");

                    if (roles.Contains("Employee"))
                        return RedirectToAction("Profile", "Employee");
                }

                // fallback
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(viewModel);
        }

        // Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        #endregion
        #region Forget Password

        // Step 1: Show form to enter email
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // Step 2: Send OTP to email
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            if (!IsMailSettingsConfigured())
            {
                _logger.LogWarning("ForgotPassword blocked because SMTP settings are not configured.");
                ModelState.AddModelError(string.Empty, "Email service is not configured. Please configure SMTP settings before sending OTP.");
                return View(dto);
            }

            try
            {
                await _passwordResetManager.SendOtpAsync(dto.Email);
                ViewBag.Message = "OTP sent to your email.";
                return View("VerifyOtp", new VerifyOtpDto { Email = dto.Email });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "ForgotPassword failed because email settings are not configured.");
                ModelState.AddModelError(string.Empty, "Email service is not configured. Please configure SMTP settings before sending OTP.");
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ForgotPassword failed to send OTP to {Email}", dto.Email);
                ModelState.AddModelError(string.Empty, "Unable to send OTP right now. Please try again later.");
                return View(dto);
            }
        }

        // Step 3: Verify OTP
        [HttpGet]
        public IActionResult VerifyOtp() => View();

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var token = await _passwordResetManager.VerifyOtpAsync(dto.Email, dto.Otp);

                return RedirectToAction("ResetPassword", new { token });

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // Step 4: Reset Password
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Invalid reset token");

            return View(new ResetPasswordDto
            {
                Token = token
            });
        }



        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                if (string.IsNullOrWhiteSpace(dto.Token))
                {
                    ModelState.AddModelError("", "Invalid or expired reset token.");
                    return View(dto);
                }

                await _passwordResetManager.ResetPasswordAsync(dto.Token, dto.NewPassword);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }


        #endregion
        private bool IsMailSettingsConfigured()
        {
            return !string.IsNullOrWhiteSpace(_configuration["MailSettings:Host"])
                && !string.IsNullOrWhiteSpace(_configuration["MailSettings:Email"])
                && !string.IsNullOrWhiteSpace(_configuration["MailSettings:Password"]);
        }

        private void ParseHiringDate(string? dateString, out DateOnly? result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(dateString)) return;

            if (DateOnly.TryParse(dateString, out var dt))
                result = dt;
            else if (DateTime.TryParse(dateString, out var dt2))
                result = DateOnly.FromDateTime(dt2);
        }

        private async Task SendEmailFallbackAsync(EmployeeProfileRequest req)
        {
            try
            {
                var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    var hrUsers = await _userManager.GetUsersInRoleAsync("HR");
                    foreach (var hr in hrUsers)
                        if (!string.IsNullOrEmpty(hr?.Email)) targets.Add(hr.Email);
                }
                catch { }

                try
                {
                    var cfg = _configuration["MailSettings:HrEmails"];
                    if (!string.IsNullOrWhiteSpace(cfg))
                        foreach (var e in cfg.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                            targets.Add(e.Trim());
                }
                catch { }

                foreach (var to in targets)
                {
                    try
                    {
                        var subject = "[EMS] Employee Profile Request";
                        var body = $"A new profile request from {req.Name} ({req.Email}).\nMessage: {req.Message ?? string.Empty}";
                        await _emailSender.SendEmailAsync(to, subject, body);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed sending email to {Email}", to);
                    }
                }

                TempData["SuccessMessage"] = "Request saved and email sent to HR.";
            }
            catch { }
        }
    }
}
