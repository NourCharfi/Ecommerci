namespace GestionArticles.Controllers
{
    using GestionArticles.Models.Repositories;
    using GestionArticles.Models.Notifications;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using System.Linq;
    using System;
    using System.Collections.Generic;

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationRepository notificationRepository,
            UserManager<IdentityUser> userManager,
            ILogger<NotificationsController> logger)
        {
            _notificationRepository = notificationRepository;
            _userManager = userManager;
            _logger = logger;
        }

        // ? Projection sécurisée avec format cohérent
        private static object Project(Notification n) => new
        {
            id = n.Id,
            title = n.Title ?? string.Empty,
            message = n.Message ?? string.Empty,
            icon = n.Icon ?? "fa-bell",
            color = n.Color ?? "info",
            actionUrl = n.ActionUrl ?? "#",
            isRead = n.IsRead,
            createdAt = n.CreatedAt.ToString("o"),  // ISO 8601 format
            type = n.Type.ToString()  // String representation
        };

        // GET: api/notifications/health
        [HttpGet("health")]
        public IActionResult Health() => Ok(new { status = "ok", time = DateTime.UtcNow });

        // GET: api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            int count;
            try
            {
                count = _notificationRepository.GetUnreadCount(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUnreadCount error");
                count = 0;
            }
            return Ok(new { count });
        }

        // GET: api/notifications/list?count=100
        // ? Toujours retourne un array
        [HttpGet("list")]
        public async Task<IActionResult> GetNotifications([FromQuery] int count = 10)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                count = Math.Clamp(count, 1, 200);

                IList<Notification> notifications;
                try
                {
                    notifications = _notificationRepository.GetByUserId(user.Id, count) ?? new List<Notification>();
                }
                catch (Exception repoEx)
                {
                    _logger.LogError(repoEx, "Repository error GetByUserId id={UserId}", user.Id);
                    notifications = new List<Notification>();
                }

                var result = notifications.Select(Project).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetNotifications failed");
                return Ok(Array.Empty<object>());  // ? Toujours un array
            }
        }

        // GET: api/notifications/unread
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                IList<Notification> notifications;
                try
                {
                    notifications = _notificationRepository.GetUnreadByUserId(user.Id) ?? new List<Notification>();
                }
                catch (Exception repoEx)
                {
                    _logger.LogError(repoEx, "Repository error GetUnreadByUserId id={UserId}", user.Id);
                    notifications = new List<Notification>();
                }

                var result = notifications.Select(Project).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUnreadNotifications failed");
                return Ok(Array.Empty<object>());  // ? Toujours un array
            }
        }

        // POST: api/notifications/{id}/read
        [HttpPost("{id}/read")]
        public IActionResult MarkAsRead(int id)
        {
            try
            {
                _notificationRepository.MarkAsRead(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarkAsRead error id={Id}", id);
                return Ok(new { success = false });
            }
        }

        // POST: api/notifications/read-all
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
            try
            {
                _notificationRepository.MarkAllAsRead(user.Id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarkAllAsRead error user={UserId}", user.Id);
                return Ok(new { success = false });
            }
        }

        // DELETE: api/notifications/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteNotification(int id)
        {
            try
            {
                _notificationRepository.Delete(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteNotification error id={Id}", id);
                return Ok(new { success = false });
            }
        }
    }

    // ? Admin Notifications - Route clara
    [Authorize(Roles = "Admin")]
    [Route("api/notifications/admin")]
    [ApiController]
    public class AdminNotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<AdminNotificationsController> _logger;

        public AdminNotificationsController(
            INotificationRepository notificationRepository,
            ILogger<AdminNotificationsController> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        private static object Project(Notification n) => new
        {
            id = n.Id,
            title = n.Title ?? string.Empty,
            message = n.Message ?? string.Empty,
            icon = n.Icon ?? "fa-bell",
            color = n.Color ?? "info",
            actionUrl = n.ActionUrl ?? "#",
            isRead = n.IsRead,
            createdAt = n.CreatedAt.ToString("o"),
            type = n.Type.ToString()
        };

        // GET: api/notifications/admin/unread-count
        [HttpGet("unread-count")]
        public IActionResult GetUnreadAdminCount()
        {
            int count;
            try
            {
                count = _notificationRepository.GetUnreadAdminCount();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUnreadAdminCount error");
                count = 0;
            }
            return Ok(new { count });
        }

        // GET: api/notifications/admin/list
        [HttpGet("list")]
        public IActionResult GetAdminNotifications([FromQuery] int count = 20)
        {
            try
            {
                count = Math.Clamp(count, 1, 200);
                IList<Notification> notifications;
                try
                {
                    notifications = _notificationRepository.GetAdminNotifications(count) ?? new List<Notification>();
                }
                catch (Exception repoEx)
                {
                    _logger.LogError(repoEx, "Repository error GetAdminNotifications");
                    notifications = new List<Notification>();
                }
                var result = notifications.Select(Project).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAdminNotifications failed");
                return Ok(Array.Empty<object>());  // ? Toujours un array
            }
        }

        // GET: api/notifications/admin/unread
        [HttpGet("unread")]
        public IActionResult GetUnreadAdminNotifications()
        {
            try
            {
                IList<Notification> notifications;
                try
                {
                    notifications = _notificationRepository.GetUnreadAdminNotifications() ?? new List<Notification>();
                }
                catch (Exception repoEx)
                {
                    _logger.LogError(repoEx, "Repository error GetUnreadAdminNotifications");
                    notifications = new List<Notification>();
                }
                var result = notifications.Select(Project).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUnreadAdminNotifications failed");
                return Ok(Array.Empty<object>());  // ? Toujours un array
            }
        }

        // POST: api/notifications/admin/{id}/read
        [HttpPost("{id}/read")]
        public IActionResult MarkAsRead(int id)
        {
            try
            {
                _notificationRepository.MarkAsRead(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin MarkAsRead error id={Id}", id);
                return Ok(new { success = false });
            }
        }

        // DELETE: api/notifications/admin/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteNotification(int id)
        {
            try
            {
                _notificationRepository.Delete(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin DeleteNotification error id={Id}", id);
                return Ok(new { success = false });
            }
        }
    }
}
