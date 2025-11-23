using GestionArticles.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GestionArticles.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Users")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Liste tous les utilisateurs
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
        {
            var users = _userManager.Users.OrderByDescending(u => u.Id).ToList();

            // Pagination
            var paginatedUsers = users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPages = (int)Math.Ceiling(users.Count() / (double)pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalUsers = users.Count();

            return View(paginatedUsers);
        }

        /// <summary>
        /// Détails d'un utilisateur
        /// </summary>
        [HttpGet("{id}/details")]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;

            return View(user);
        }

        /// <summary>
        /// Éditer un utilisateur
        /// </summary>
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.AvailableRoles = await _roleManager.Roles.ToListAsync();
            ViewBag.UserRoles = roles;

            return View(user);
        }

        /// <summary>
        /// Mettre à jour un utilisateur
        /// </summary>
        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(string id, IdentityUser model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.Email = model.Email;
            user.UserName = model.UserName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {id} updated successfully");
                TempData["Success"] = "User updated successfully";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }

        /// <summary>
        /// Supprimer un utilisateur
        /// </summary>
        [HttpGet("{id}/delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return View(user);
        }

        /// <summary>
        /// Confirmer la suppression
        /// </summary>
        [HttpPost("{id}/delete")]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {id} deleted successfully");
                TempData["Success"] = "User deleted successfully";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }

        /// <summary>
        /// Assigner des rôles à un utilisateur
        /// </summary>
        [HttpPost("{id}/assign-roles")]
        public async Task<IActionResult> AssignRoles(string id, [FromForm] List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Supprimer les rôles actuels
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                TempData["Error"] = "Failed to remove current roles";
                return RedirectToAction(nameof(Edit), new { id });
            }

            // Ajouter les nouveaux rôles
            if (roles != null && roles.Count > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, roles);
                if (!addResult.Succeeded)
                {
                    TempData["Error"] = "Failed to assign roles";
                    return RedirectToAction(nameof(Edit), new { id });
                }
            }

            _logger.LogInformation($"Roles updated for user {id}");
            TempData["Success"] = "Roles assigned successfully";
            return RedirectToAction(nameof(Edit), new { id });
        }

        /// <summary>
        /// Réinitialiser le mot de passe d'un utilisateur
        /// </summary>
        [HttpGet("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return View(user);
        }

        /// <summary>
        /// Confirmer la réinitialisation du mot de passe
        /// </summary>
        [HttpPost("{id}/reset-password")]
        [ActionName("ResetPassword")]
        public async Task<IActionResult> ResetPasswordConfirmed(string id, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError("", "Password is required");
                return View(user);
            }

            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                TempData["Error"] = "Failed to reset password";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (!addPasswordResult.Succeeded)
            {
                TempData["Error"] = "Failed to set new password";
                return RedirectToAction(nameof(Edit), new { id });
            }

            _logger.LogInformation($"Password reset for user {id}");
            TempData["Success"] = "Password reset successfully";
            return RedirectToAction(nameof(Edit), new { id });
        }
    }
}
