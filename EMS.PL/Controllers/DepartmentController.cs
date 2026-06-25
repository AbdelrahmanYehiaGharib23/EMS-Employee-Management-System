using EMS.BLL.Models.Dto.DepartmentDto;
using EMS.BLL.Services.DepartmentServices;
using EMS.PL.ViewModels.Department;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EMS.PL.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<DepartmentController> _logger;
        private readonly IWebHostEnvironment _environment;

        public DepartmentController(IDepartmentService departmentService, ILogger<DepartmentController> logger, IWebHostEnvironment environment)
        {
            _departmentService = departmentService;
            _logger = logger;
            _environment = environment;
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> Index()
        {
            var departments = await _departmentService.GetDepartmentsAsync();
            return View(departments);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> Create(DepartmentViewModel departmentViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var departmentDto = new CreateDepartmentDto(
                        departmentViewModel.Name,
                        departmentViewModel.Code,
                        departmentViewModel.Description,
                        departmentViewModel.DateOfCreation);

                    int result = await _departmentService.CreateDepartmentAsync(departmentDto);
                    if (result > 0)
                        TempData["Message"] = $"Department {departmentViewModel.Name} created successfully";
                    else
                        TempData["Message"] = $"Failed to create department {departmentViewModel.Name}";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    if (_environment.IsDevelopment())
                        ModelState.AddModelError(string.Empty, ex.Message);
                    else
                        _logger.LogError(ex.Message);
                }
            }
            return View(departmentViewModel);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue) return BadRequest();
            var department = await _departmentService.GetDepartmentByIdAsync(id.Value);
            if (department is null) return NotFound();
            return View(department);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async System.Threading.Tasks.Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue) return BadRequest();
            var department = await _departmentService.GetDepartmentByIdAsync(id.Value);
            if (department is null) return NotFound();

            var viewModel = new DepartmentViewModel()
            {
                Name = department.Name ?? string.Empty,
                Code = department.Code ?? string.Empty,
                Description = department.Description,
                DateOfCreation = department.DateOfCreation,
            };
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> Edit([FromRoute] int id, DepartmentViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var updateDto = new UpdateDepartmentDto()
                    {
                        Id = id,
                        Name = viewModel.Name,
                        Code = viewModel.Code,
                        Description = viewModel.Description,
                        DateOfCreation = viewModel.DateOfCreation,
                    };
                    int result = await _departmentService.UpdateDepartmentAsync(updateDto);
                    if (result > 0) return RedirectToAction(nameof(Index));
                    ModelState.AddModelError(string.Empty, "Failed to update department");
                }
                catch (Exception ex)
                {
                    if (_environment.IsDevelopment())
                        ModelState.AddModelError(string.Empty, ex.Message);
                    else
                    {
                        _logger.LogError(ex.Message);
                        return View("ErrorView", ex);
                    }
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async System.Threading.Tasks.Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue) return BadRequest();
            var department = await _departmentService.GetDepartmentByIdAsync(id.Value);
            if (department == null) return NotFound();
            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id == 0) return BadRequest();

            try
            {
                bool deleted = await _departmentService.DeleteDepartmentAsync(id);
                if (deleted)
                {
                    TempData["SuccessMessage"] = "Department deleted successfully";
                    _logger.LogInformation($"Department deleted: ID={id}");
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "Failed to delete department");
                var department = await _departmentService.GetDepartmentByIdAsync(id);
                return View(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department");
                return View("ErrorView", ex);
            }
        }
    }
}
