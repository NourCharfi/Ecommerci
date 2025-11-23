using GestionArticles.ViewModels.Auth;
using GestionArticles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GestionArticles.Controllers
{

    public class AccountController : Controller
    {
        // GET: AccountController
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
            INotificationService notificationService, ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.UserName, // utiliser le nom d'utilisateur saisi
                    Email = model.Email,
                    PhoneNumber = model.Phone
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                    _notificationService.NotifyAdminNewUser(user.Id, user.Email);
                    _logger.LogInformation($"👤 Nouvel utilisateur inscrit: {user.UserName} / {user.Email} (ID: {user.Id})");
                    await signInManager.SignInAsync(user, isPersistent: false);
                    TempData["SuccessMessage"] = $"✅ Bienvenue {user.UserName}!";
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Autoriser saisie email OU nom d'utilisateur
                IdentityUser? user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    user = await userManager.FindByNameAsync(model.Email); // fallback username
                }
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Identifiant ou mot de passe incorrect.");
                    return View(model);
                }

                var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return !string.IsNullOrEmpty(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Identifiant ou mot de passe incorrect.");
            }
            return View(model);
        }

        // --- New: Profile edit actions ---
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");
            var model = new EditProfileViewModel
            {
                Email = user.Email ?? string.Empty,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber
            };
            return View("Edit", model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(EditProfileViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }

            // Email modifié
            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmailResult = await userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var e in setEmailResult.Errors) ModelState.AddModelError(string.Empty, e.Description);
                    return View("Edit", model);
                }
                // MAJ username si non saisi autrement
                if (string.IsNullOrWhiteSpace(model.UserName))
                {
                    user.UserName = model.Email;
                    await userManager.SetUserNameAsync(user, model.Email);
                }
            }

            // Username facultatif distinct de l'email
            if (!string.IsNullOrWhiteSpace(model.UserName) && !string.Equals(user.UserName, model.UserName, StringComparison.Ordinal))
            {
                var setUserResult = await userManager.SetUserNameAsync(user, model.UserName);
                if (!setUserResult.Succeeded)
                {
                    foreach (var e in setUserResult.Errors) ModelState.AddModelError(string.Empty, e.Description);
                    return View("Edit", model);
                }
            }

            // Téléphone facultatif
            if (!string.Equals(user.PhoneNumber, model.PhoneNumber, StringComparison.Ordinal))
            {
                var phoneResult = await userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!phoneResult.Succeeded)
                {
                    foreach (var e in phoneResult.Errors) ModelState.AddModelError(string.Empty, e.Description);
                    return View("Edit", model);
                }
            }

            // Changement de mot de passe facultatif
            var hasNewPwd = !string.IsNullOrWhiteSpace(model.NewPassword) || !string.IsNullOrWhiteSpace(model.ConfirmNewPassword);
            if (hasNewPwd)
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    ModelState.AddModelError(string.Empty, "Le mot de passe actuel est requis uniquement si vous changez le mot de passe.");
                    return View("Edit", model);
                }
                if (string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    ModelState.AddModelError(string.Empty, "Le nouveau mot de passe est requis.");
                    return View("Edit", model);
                }
                if (string.IsNullOrWhiteSpace(model.ConfirmNewPassword))
                {
                    ModelState.AddModelError(string.Empty, "La confirmation du nouveau mot de passe est requise.");
                    return View("Edit", model);
                }
                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    ModelState.AddModelError(string.Empty, "Le nouveau mot de passe et la confirmation ne correspondent pas.");
                    return View("Edit", model);
                }
                var changePwdResult = await userManager.ChangePasswordAsync(user, model.CurrentPassword!, model.NewPassword!);
                if (!changePwdResult.Succeeded)
                {
                    foreach (var e in changePwdResult.Errors) ModelState.AddModelError(string.Empty, e.Description);
                    return View("Edit", model);
                }
            }

            await signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Profil mis à jour avec succès.";
            return RedirectToAction("Profile");
        }
    }
}
