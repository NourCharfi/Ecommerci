using GestionArticles.Models.Orders;
using GestionArticles.Models.Repositories;
using GestionArticles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GestionArticles.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderRepository orderRepo;
        private readonly UserManager<IdentityUser> userManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderRepository orderRepo, UserManager<IdentityUser> userManager, 
            INotificationService notificationService, ILogger<OrdersController> logger)
        {
            this.orderRepo = orderRepo;
            this.userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
        }

        // Admin / Manager: list all orders
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Index()
        {
            var list = orderRepo.GetAll();
            return View(list);
        }

        // Admin / Manager: change status
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult UpdateStatus(int id, OrderStatus status)
        {
            var order = orderRepo.GetById(id);
            if (order == null) return NotFound();

            var oldStatus = order.Status;
            order.Status = status;
            orderRepo.Update(order);

            // ? AJOUTER: Notifier l'utilisateur du changement de statut
            switch (status)
            {
                case OrderStatus.Paid:
                    _notificationService.NotifyOrderPaid(order.Id, order.UserId);
                    _logger.LogInformation($"?? Commande {order.Id} PAYÉE pour {order.CustomerName}");
                    TempData["SuccessMessage"] = $"?? Commande #{order.Id} marquée comme payée.";
                    break;
                case OrderStatus.Confirmed:
                    _notificationService.NotifyOrderConfirmed(order.Id, order.UserId);
                    _logger.LogInformation($"? Commande {order.Id} CONFIRMÉE pour {order.CustomerName}");
                    TempData["SuccessMessage"] = $"? Commande #{order.Id} confirmée.";
                    break;
                case OrderStatus.Shipping:
                    _notificationService.NotifyOrderShipping(order.Id, order.UserId);
                    _logger.LogInformation($"?? Commande {order.Id} EN EXPÉDITION pour {order.CustomerName}");
                    TempData["SuccessMessage"] = $"?? Commande #{order.Id} en expédition.";
                    break;
                case OrderStatus.Delivered:
                    _notificationService.NotifyOrderDelivered(order.Id, order.UserId);
                    _logger.LogInformation($"?? Commande {order.Id} LIVRÉE à {order.CustomerName}");
                    TempData["SuccessMessage"] = $"?? Commande #{order.Id} livrée.";
                    break;
                case OrderStatus.Processing:
                    _logger.LogInformation($"? Commande {order.Id} EN TRAITEMENT");
                    TempData["SuccessMessage"] = $"? Commande #{order.Id} en traitement.";
                    break;
                case OrderStatus.Preparing:
                    _logger.LogInformation($"?? Commande {order.Id} EN PRÉPARATION");
                    TempData["SuccessMessage"] = $"?? Commande #{order.Id} en préparation.";
                    break;
            }

            _logger.LogInformation($"Statut changé: {oldStatus} ? {status}");

            return RedirectToAction(nameof(Index));
        }

        // Authenticated users: view their orders
        [Authorize]
        public IActionResult MyOrders()
        {
            var userId = userManager.GetUserId(User);
            var list = orderRepo.GetByUserId(userId);
            return View(list);
        }

        // Authenticated users: view one order (owner) ; Admin/Manager can view any
        [Authorize]
        public IActionResult Details(int id)
        {
            var order = orderRepo.GetById(id);
            if (order == null) return NotFound();

            var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
            if (!isAdminOrManager && order.UserId != userManager.GetUserId(User))
            {
                return Forbid();
            }

            return View(order);
        }

        // Printable view - same access rules as Details
        [Authorize]
        public IActionResult Print(int id)
        {
            var order = orderRepo.GetById(id);
            if (order == null) return NotFound();

            var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
            if (!isAdminOrManager && order.UserId != userManager.GetUserId(User))
            {
                return Forbid();
            }

            return View("Print", order);
        }
    }
}
