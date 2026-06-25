using AutoMapper;
using EMS.DAL.Entities.IdentityModel;
using EMS.PL.ViewModels.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMS.PL.Controllers.IdentityController
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                var userEntities = await _userManager.Users.ToListAsync();
                var users = new List<UserViewModel>();
                
                foreach (var u in userEntities)
                {
                    var roles = await _userManager.GetRolesAsync(u);
                    users.Add(new UserViewModel
                    {
                        Id = u.Id,
                        Email = u.Email ?? string.Empty,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        UserName = u.UserName ?? string.Empty,
                        Roles = roles
                    });
                }

                return View(users);
            }

            var user = await _userManager.FindByEmailAsync(email.Trim());
            if (user is null) return View(Enumerable.Empty<UserViewModel>());

            var model = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName ?? string.Empty,
                Roles = await _userManager.GetRolesAsync(user)
            };

            return View(new List<UserViewModel> { model });
        }

        public async Task<IActionResult> Details(string? id, string viewName = nameof(Details))
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var model = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName ?? string.Empty
            };

            return View(viewName, model);
        }

        public async Task<IActionResult> Edit(string? id)
            => await Details(id, nameof(Edit));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string? id, UserViewModel model)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            if (id != model.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user is null)
                    return NotFound();

                user.FirstName = model.FirstName ?? string.Empty;
                user.LastName = model.LastName;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(string? id)
            => await Details(id, nameof(Delete));

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            try
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while deleting the user.");
            }

            var model = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName ?? string.Empty
            };

            return View(nameof(Delete), model);
        }
    }
}
