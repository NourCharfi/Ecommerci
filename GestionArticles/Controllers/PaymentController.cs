using GestionArticles.Models;
using GestionArticles.Models.Repositories;
using GestionArticles.Services;
using GestionArticles.ViewModels.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace GestionArticles.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderRepository _orderRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, IOrderRepository orderRepository,
            INotificationService notificationService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _orderRepository = orderRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// GET: Afficher le formulaire de paiement (redirection vers Stripe Checkout)
        /// </summary>
        [HttpGet]
        public IActionResult Checkout(int orderId, float amount)
        {
            var order = _orderRepository.GetById(orderId);
            if (order == null)
            {
                _logger.LogWarning($"Order not found: {orderId}");
                TempData["ErrorMessage"] = "Commande non trouvée.";
                return RedirectToAction("Index", "Panier");
            }

            var paymentModel = new PaymentViewModel
            {
                OrderId = orderId,
                TotalAmount = amount > 0 ? amount : (float)(order.TotalAmount + order.DeliveryFee),
                Email = order.Email
            };

            return View(paymentModel);
        }

        // Paiement MANUEL (sans Stripe) -------------------------------------------
        /// <summary>
        /// GET: Formulaire de paiement manuel
        /// </summary>
        [HttpGet]
        public IActionResult Manual(int orderId)
        {
            var order = _orderRepository.GetById(orderId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Commande introuvable.";
                return RedirectToAction("Index", "Panier");
            }

            return View(new PaymentViewModel
            {
                OrderId = orderId,
                TotalAmount = (float)(order.TotalAmount + order.DeliveryFee),
                Email = order.Email
            });
        }

        /// <summary>
        /// POST: Confirmer le paiement manuel
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Manual(PaymentViewModel model)
        {
            if (!ModelState.IsValid || model.OrderId <= 0)
            {
                TempData["ErrorMessage"] = "Données invalides.";
                return View(model);
            }

            var order = _orderRepository.GetById(model.OrderId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Commande introuvable.";
                return View(model);
            }

            // Simulation basique (NE PAS UTILISER EN PRODUCTION)
            if (string.IsNullOrWhiteSpace(model.CardNumber) || model.CardNumber!.Replace(" ", "").Length < 12)
            {
                ModelState.AddModelError("CardNumber", "Numéro de carte invalide.");
                return View(model);
            }
            if (string.IsNullOrWhiteSpace(model.CardExpiry) || !model.CardExpiry!.Contains('/'))
            {
                ModelState.AddModelError("CardExpiry", "Date expiration invalide (MM/AA). ");
                return View(model);
            }
            if (string.IsNullOrWhiteSpace(model.CardCVC) || model.CardCVC!.Length < 3)
            {
                ModelState.AddModelError("CardCVC", "CVC invalide.");
                return View(model);
            }

            // Marquer la commande comme payée (simulation paiement réussi)
            order.Status = Models.Orders.OrderStatus.Paid;
            _orderRepository.Update(order);
            _notificationService.NotifyOrderPaid(order.Id, order.UserId);
            _notificationService.NotifyAdminPaymentReceived(order.Id, (decimal)order.TotalAmount);
            _logger.LogInformation($"? Paiement manuel simulé pour commande {order.Id}");
            TempData["SuccessMessage"] = "Paiement manuel confirmé.";

            // Rediriger vers succès sans session Stripe
            return RedirectToAction("Success", new { orderId = order.Id });
        }

        /// <summary>
        /// POST: Créer une Session Checkout Stripe (hébergée par Stripe)
        /// </summary>
        [HttpPost("Payment/CreateCheckoutSession")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutRequest request)
        {
            try
            {
                if (request == null || request.OrderId <= 0) return BadRequest(new { error = "OrderId invalide" });
                int orderId = request.OrderId; float amount = request.Amount; string email = request.Email;
                if (string.IsNullOrEmpty(email)) return BadRequest(new { error = "Email manquant" });
                var order = _orderRepository.GetById(orderId);
                if (order == null) return NotFound(new { error = "Commande non trouvée" });
                var domainUrl = $"{Request.Scheme}://{Request.Host}";
                _logger.LogInformation($"Création session Stripe - Commande {orderId}, Montant: {amount}€");
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmountDecimal = (decimal?)Math.Round((decimal)amount * 100m, 0),
                                Currency = "eur",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"Commande ECommerci.tn #{orderId}",
                                    Description = $"{order.Items?.Count ?? 0} article(s)"
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    CustomerEmail = email,
                    SuccessUrl = $"{domainUrl}/Payment/Success?orderId={orderId}&sessionId=" + "{CHECKOUT_SESSION_ID}",
                    CancelUrl = $"{domainUrl}/Payment/Cancel?orderId={orderId}",
                    Metadata = new Dictionary<string, string> { { "orderId", orderId.ToString() } }
                };
                var service = new SessionService();
                var session = await service.CreateAsync(options);
                _logger.LogInformation($"Session Stripe créée: {session.Id}");
                return Json(new { success = true, sessionId = session.Id });
            }
            catch (StripeException ex)
            { _logger.LogError($"Erreur Stripe: {ex.Message}"); return BadRequest(new { error = "Erreur Stripe: " + ex.Message }); }
            catch (Exception ex)
            { _logger.LogError($"Erreur création session: {ex.Message}"); return BadRequest(new { error = "Erreur: " + ex.Message }); }
        }

        // DTO pour la requête
        public class CreateCheckoutRequest { public int OrderId { get; set; } public float Amount { get; set; } public string Email { get; set; } }

        /// <summary>
        /// GET: Succès du paiement (redirection depuis Stripe Checkout)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Success(int orderId, string? sessionId)
        {
            var order = _orderRepository.GetById(orderId);
            if (order == null) { TempData["ErrorMessage"] = "Commande introuvable."; return RedirectToAction("Index", "Panier"); }

            if (!string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    var service = new SessionService();
                    var session = await service.GetAsync(sessionId);
                    if (session.PaymentStatus == "paid")
                    {
                        order.Status = Models.Orders.OrderStatus.Paid;
                        _orderRepository.Update(order);
                        _notificationService.NotifyOrderPaid(orderId, order.UserId);
                        _notificationService.NotifyAdminPaymentReceived(orderId, (decimal)order.TotalAmount);
                        TempData["SuccessMessage"] = $"Paiement réussi! Votre commande #{orderId} est payée.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Le paiement n'a pas pu être confirmé.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erreur vérification paiement: {ex.Message}");
                    TempData["ErrorMessage"] = $"Erreur: {ex.Message}";
                }
            }
            else
            {
                // Paiement manuel déjà confirmé dans action Manual
                if (order.Status == Models.Orders.OrderStatus.Paid)
                    TempData["SuccessMessage"] = $"Paiement manuel confirmé pour la commande #{orderId}.";
            }
            return View(order);
        }

        /// <summary>
        /// GET: Annulation du paiement
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Cancel(int orderId)
        {
            _logger.LogWarning($"Paiement annulé pour commande {orderId}");
            TempData["ErrorMessage"] = "Vous avez annulé le paiement.";
            return View();
        }
    }
}
