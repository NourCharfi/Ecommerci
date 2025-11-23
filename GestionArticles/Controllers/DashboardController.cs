using GestionArticles.Models.Repositories;
using GestionArticles.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GestionArticles.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IAuditLogRepository _auditRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IAuditLogRepository auditRepository,
            UserManager<IdentityUser> userManager,
            ILogger<DashboardController> logger)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _auditRepository = auditRepository;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard personnalisé selon le rôle
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Vérifier le rôle et afficher le dashboard approprié
            if (User.IsInRole("Admin"))
            {
                return await AdminDashboard();
            }
            else if (User.IsInRole("Manager"))
            {
                return await ManagerDashboard();
            }
            else
            {
                // Dashboard client (rediriger vers les commandes)
                return RedirectToAction("MyOrders", "Orders");
            }
        }

        /// <summary>
        /// Dashboard Admin - Vue complète
        /// </summary>
        [Authorize(Roles = "Admin")]
        private async Task<IActionResult> AdminDashboard()
        {
            var allProducts = _productRepository.GetAll();
            var allOrders = _orderRepository.GetAll();
            var deletedProducts = _productRepository.GetDeleted();
            var recentAuditLogs = _auditRepository.GetAll(20);

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

            var model = new AdminDashboardViewModel
            {
                // Stats générales
                TotalProducts = allProducts.Count,
                TotalOrders = allOrders.Count,
                TotalRevenue = (decimal)allOrders.Sum(o => o.TotalAmount),
                TotalUsers = adminUsers.Count,

                // Produits
                ProductsLowStock = allProducts.Count(p => p.QteStock > 0 && p.QteStock <= 20),
                ProductsOutOfStock = allProducts.Count(p => p.QteStock == 0),
                ProductsDeleted = deletedProducts.Count,

                // Commandes
                OrdersPending = allOrders.Count(o => o.Status.ToString() == "Processing"),
                OrdersShipping = allOrders.Count(o => o.Status.ToString() == "Confirmed"),
                OrdersDelivered = allOrders.Count(o => o.Status.ToString() == "Delivered"),

                // Commandes récentes
                RecentOrders = allOrders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .Select(o => new OrderSummary
                    {
                        Id = o.Id,
                        CustomerName = o.CustomerName,
                        TotalAmount = (decimal)o.TotalAmount,
                        OrderDate = o.OrderDate,
                        Status = o.Status.ToString()
                    })
                    .ToList(),

                // Audit logs récents
                RecentAuditLogs = recentAuditLogs
                    .Select(a => new AuditSummary
                    {
                        UserEmail = a.UserEmail,
                        Action = a.ActionType.ToString(),
                        Entity = $"{a.EntityType} - {a.EntityName}",
                        Timestamp = a.Timestamp
                    })
                    .ToList()
            };

            _logger.LogInformation("Admin dashboard accessed");
            return View("AdminDashboard", model);
        }

        /// <summary>
        /// Dashboard Manager - Vue restreinte
        /// </summary>
        [Authorize(Roles = "Manager")]
        private Task<IActionResult> ManagerDashboard()
        {
            var allProducts = _productRepository.GetAll();
            var allOrders = _orderRepository.GetAll();

            var model = new ManagerDashboardViewModel
            {
                // Stats (pas d'utilisateurs)
                TotalProducts = allProducts.Count,
                TotalOrders = allOrders.Count,
                TotalRevenue = (decimal)allOrders.Sum(o => o.TotalAmount),

                // Alertes
                ProductsLowStock = allProducts.Count(p => p.QteStock > 0 && p.QteStock <= 20),
                OrdersPending = allOrders.Count(o => o.Status.ToString() == "Processing"),

                // Commandes récentes
                RecentOrders = allOrders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .Select(o => new OrderSummary
                    {
                        Id = o.Id,
                        CustomerName = o.CustomerName,
                        TotalAmount = (decimal)o.TotalAmount,
                        OrderDate = o.OrderDate,
                        Status = o.Status.ToString()
                    })
                    .ToList()
            };

            _logger.LogInformation("Manager dashboard accessed");
            return Task.FromResult<IActionResult>(View("ManagerDashboard", model));
        }
    }
}
