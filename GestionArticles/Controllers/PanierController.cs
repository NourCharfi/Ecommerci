using System;
using System.Linq;
using System.Threading.Tasks;
using GestionArticles.Models;
using GestionArticles.Models.Help;
using GestionArticles.Models.Repositories;
using GestionArticles.Models.Orders;
using GestionArticles.Services;
using GestionArticles.ViewModels.Panier;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GestionArticles.Models.Promotions;

namespace GestionArticles.Controllers
{
    public class PanierController : Controller
    {
        readonly IProductRepository productRepository;
        readonly IOrderRepository orderRepository;
        private readonly UserManager<IdentityUser> userManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PanierController> _logger;
        private readonly IDiscountService _discountService;

        public PanierController(IProductRepository productRepository,
            IOrderRepository orderRepository,
            UserManager<IdentityUser> userManager,
            INotificationService notificationService,
            ILogger<PanierController> logger,
            IDiscountService discountService)
        {
            this.productRepository = productRepository;
            this.orderRepository = orderRepository;
            this.userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
            _discountService = discountService;
        }

        // GET: /Panier/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            }
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            }

            var cartItems = ListeCart.Instance.Items.ToList();
            var viewModel = new OrderViewModel();

            decimal discountedSubtotal = 0m;
            bool hasFreeShipping = false;

            foreach (var item in cartItems)
            {
                var discount = _discountService.GetApplicableDiscount(item.Prod);
                var originalUnit = (decimal)item.Prod.Price;
                var discountedUnit = _discountService.CalculateDiscountedPrice(item.Prod, originalUnit);
                var rowTotal = _discountService.CalculateRowTotal(item.Prod, item.quantite, originalUnit);
                var savings = (originalUnit * item.quantite) - rowTotal;
                string? discountType = discount?.Type.ToString();

                if (_discountService.HasFreeShipping(item.Prod))
                {
                    hasFreeShipping = true;
                }

                discountedSubtotal += rowTotal;

                viewModel.CartItems.Add(new CartItemViewModel
                {
                    ProductName = item.Prod.Name,
                    Quantity = item.quantite,
                    Price = item.Prod.Price,
                    DiscountedUnitPrice = (float)discountedUnit != item.Prod.Price ? (float)discountedUnit : null,
                    RowTotal = (float)rowTotal,
                    Savings = savings > 0 ? (float)savings : null,
                    DiscountType = discountType
                });
            }

            viewModel.TotalAmount = (float)discountedSubtotal;
            viewModel.DeliveryFee = hasFreeShipping ? 0 : 10; // 10 DT frais livraison
            return View(viewModel);
        }

        // POST : /Panier/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = user_manager_Get();
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Utilisateur non authentifié.";
                    return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
                }

                var cartItems = ListeCart.Instance.Items.ToList();
                model.CartItems.Clear();
                decimal discountedSubtotal = 0m;
                bool hasFreeShipping = false;

                foreach (var item in cartItems)
                {
                    var discount = _discountService.GetApplicableDiscount(item.Prod);
                    var originalUnit = (decimal)item.Prod.Price;
                    var discountedUnit = _discountService.CalculateDiscountedPrice(item.Prod, originalUnit);
                    var rowTotal = _discountService.CalculateRowTotal(item.Prod, item.quantite, originalUnit);
                    var savings = (originalUnit * item.quantite) - rowTotal;
                    string? discountType = discount?.Type.ToString();

                    if (_discountService.HasFreeShipping(item.Prod))
                    {
                        hasFreeShipping = true;
                    }

                    discountedSubtotal += rowTotal;

                    model.CartItems.Add(new CartItemViewModel
                    {
                        ProductName = item.Prod.Name,
                        Quantity = item.quantite,
                        Price = item.Prod.Price,
                        DiscountedUnitPrice = (float)discountedUnit != item.Prod.Price ? (float)discountedUnit : null,
                        RowTotal = (float)rowTotal,
                        Savings = savings > 0 ? (float)savings : null,
                        DiscountType = discountType
                    });
                }

                model.TotalAmount = (float)discountedSubtotal;
                var deliveryFee = hasFreeShipping ? 0f : 10f; // 10 DT
                float finalTotal = model.TotalAmount + deliveryFee;

                var order = new Order
                {
                    CustomerName = user.UserName,
                    Email = user.Email,
                    Address = model.Address,
                    TotalAmount = finalTotal,
                    OrderDate = DateTime.Now,
                    UserId = user.Id,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    DeliveryFee = deliveryFee,
                    PaymentMethod = model.PaymentMethod ?? "COD",
                    Items = model.CartItems.Select(ci => new OrderItem
                    {
                        ProductName = ci.ProductName,
                        Quantity = ci.Quantity,
                        Price = ci.Price
                    }).ToList(),
                    Status = model.PaymentMethod == "CARD" ? OrderStatus.Processing : (model.PaymentMethod == "PAYPAL" ? OrderStatus.Processing : OrderStatus.Confirmed)
                };

                orderRepository.Add(order);

                foreach (var item in cartItems)
                {
                    var prod = productRepository.GetById(item.Prod.ProductId);
                    if (prod != null)
                    {
                        prod.QteStock -= item.quantite;
                        if (prod.QteStock < 0) prod.QteStock = 0;
                        productRepository.Update(prod);
                    }
                }

                ListeCart.Instance.Items.Clear();

                _notificationService.NotifyOrderCreated(order.Id, user.Id);
                _logger.LogInformation($"✅ Nouvelle commande créée: #{order.Id} - Client: {order.CustomerName} - Total: {finalTotal} DT");

                if (finalTotal > 500000)
                {
                    _notificationService.NotifyAdminHighValueOrder(order.Id, (decimal)finalTotal);
                    _logger.LogWarning($"💰 COMMANDE VIP: #{order.Id} - Montant: {finalTotal} DT");
                }

                if (model.PaymentMethod == "CARD" || model.PaymentMethod == "PAYPAL")
                {
                    return RedirectToAction("Checkout", "Payment", new { orderId = order.Id, amount = finalTotal, paymentMethod = model.PaymentMethod });
                }

                return RedirectToAction("Confirmation", new { orderId = order.Id });
            }
            TempData["ErrorMessage"] = "Une erreur est survenue. Veuillez vérifier les informations.";
            return View(model);
        }

        private IdentityUser user_manager_Get() => userManager.GetUserAsync(User).Result;

        public IActionResult Confirmation(int orderId)
        {
            var order = orderRepository.GetById(orderId);
            return View(order);
        }

        // GET: /Panier/Promotions
        public IActionResult Promotions()
        {
            return View();
        }

        public ActionResult Index()
        {
            var items = ListeCart.Instance.Items;
            ViewBag.Liste = items;
            decimal discountedSubtotal = 0m;
            bool hasFreeShipping = false;
            foreach (var item in items)
            {
                discountedSubtotal += _discountService.CalculateRowTotal(item.Prod, item.quantite, (decimal)item.Prod.Price);
                if (_discountService.HasFreeShipping(item.Prod))
                {
                    hasFreeShipping = true;
                }
            }
            ViewBag.total = (float)discountedSubtotal;
            ViewBag.deliveryFee = hasFreeShipping ? 0 : 10; // 10 DT
            return View();
        }

        public ActionResult AddProduct(int id)
        {
            Product pp = productRepository.GetById(id);
            if (pp == null)
            {
                TempData["ErrorMessage"] = "Produit introuvable.";
                return RedirectToAction("Index");
            }
            var added = ListeCart.Instance.AddItem(pp);
            TempData[added ? "SuccessMessage" : "ErrorMessage"] = added ? "Produit ajouté au panier." : "Stock insuffisant.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult PlusProduct(int id)
        {
            var pp = productRepository.GetById(id);
            if (pp == null)
            {
                return Json(new { success = false, message = "Produit introuvable.", productId = id });
            }
            var existing = ListeCart.Instance.Items.FirstOrDefault(a => a.Prod.ProductId == id);
            var currentQty = existing != null ? existing.quantite : 0;
            if (currentQty + 1 > pp.QteStock)
            {
                return Json(new
                {
                    success = false,
                    message = "Stock insuffisant",
                    productId = id,
                    quantity = currentQty,
                    rowTotal = existing != null ? GetRowTotalWithDiscount(existing) : 0,
                    cartTotal = GetCartTotalWithDiscounts(),
                    removed = false
                });
            }
            ListeCart.Instance.AddItem(pp);
            existing = ListeCart.Instance.Items.FirstOrDefault(a => a.Prod.ProductId == id);
            var qty = existing != null ? existing.quantite : 0;
            var row = existing != null ? GetRowTotalWithDiscount(existing) : 0;
            var cartTotal = GetCartTotalWithDiscounts();
            return Json(new
            {
                success = true,
                productId = id,
                quantity = qty,
                rowTotal = row,
                cartTotal = cartTotal,
                removed = false
            });
        }

        [HttpPost]
        public ActionResult MinusProduct(int id)
        {
            var pp = productRepository.GetById(id);
            if (pp == null)
            {
                return Json(new { success = false, message = "Produit introuvable.", productId = id });
            }
            ListeCart.Instance.SetLessOneItem(pp);
            var trouve = ListeCart.Instance.Items.FirstOrDefault(a => a.Prod.ProductId == pp.ProductId);
            if (trouve != null)
            {
                var qty = trouve.quantite;
                var row = GetRowTotalWithDiscount(trouve);
                var cartTotal = GetCartTotalWithDiscounts();
                return Json(new
                {
                    success = true,
                    productId = id,
                    quantity = qty,
                    rowTotal = row,
                    cartTotal = cartTotal,
                    removed = false
                });
            }
            else
            {
                var cartTotal = GetCartTotalWithDiscounts();
                return Json(new
                {
                    success = true,
                    productId = id,
                    quantity = 0,
                    rowTotal = 0,
                    cartTotal = cartTotal,
                    removed = true
                });
            }
        }

        [HttpPost]
        public ActionResult RemoveProduct(int id)
        {
            Product pp = productRepository.GetById(id);
            ListeCart.Instance.RemoveItem(pp);
            var cartTotal = GetCartTotalWithDiscounts();
            return Json(new
            {
                productId = id,
                quantity = 0,
                rowTotal = 0,
                cartTotal = cartTotal,
                removed = true
            });
        }

        private float GetCartTotalWithDiscounts()
        {
            decimal total = 0m;
            foreach (var i in ListeCart.Instance.Items)
            {
                total += _discountService.CalculateRowTotal(i.Prod, i.quantite, (decimal)i.Prod.Price);
            }
            return (float)total;
        }

        private float GetRowTotalWithDiscount(GestionArticles.Models.Help.Item item)
        {
            var rowTotal = _discountService.CalculateRowTotal(item.Prod, item.quantite, (decimal)item.Prod.Price);
            return (float)rowTotal;
        }
    }
}
