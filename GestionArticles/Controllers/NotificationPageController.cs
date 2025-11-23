using GestionArticles.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GestionArticles.Controllers
{
    [Authorize]
    public class NotificationPageController : Controller
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<NotificationPageController> _logger;

        public NotificationPageController(
            INotificationRepository notificationRepository,
            UserManager<IdentityUser> userManager,
            ILogger<NotificationPageController> logger)
        {
            _notificationRepository = notificationRepository;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Page principale des notifications (Notification/Index.cshtml)
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return RedirectToAction("Login", "Account");

                // Récupérer notifications selon le rôle
                IList<Models.Notifications.Notification> notifications;

                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    // Admin/Manager: Notifications admin + notifications utilisateur
                    var allNotifications = _notificationRepository.GetAll() ?? new List<Models.Notifications.Notification>();
                    notifications = allNotifications
                        .Where(n => (n.IsAdmin && (n.UserId == null || n.UserId == user.Id)) || n.UserId == user.Id)
                        .OrderByDescending(n => n.CreatedAt)
                        .Take(100)
                        .ToList();

                    _logger.LogInformation($"Admin {user.Email} viewing notifications ({notifications.Count})");
                }
                else
                {
                    // Client: Uniquement notifications utilisateur
                    notifications = _notificationRepository.GetByUserId(user.Id, 100) ?? new List<Models.Notifications.Notification>();
                    _logger.LogInformation($"Client {user.Email} viewing notifications ({notifications.Count})");
                }

                return View("~/Views/Notifications/Index.cshtml", notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notifications page");
                TempData["ErrorMessage"] = "Erreur lors du chargement des notifications.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
