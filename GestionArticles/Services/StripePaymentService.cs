using Stripe;
using Stripe.Checkout;
using GestionArticles.ViewModels.Payment;

namespace GestionArticles.Services
{
    public interface IPaymentService
    {
        Task<PaymentResultViewModel> ProcessPaymentAsync(string paymentMethodId, long amountInCents, string email, string description);
        Task<Session> CreateCheckoutSessionAsync(int orderId, float amount, string customerEmail);
    }

    public class StripePaymentService : IPaymentService
    {
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(ILogger<StripePaymentService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Traiter un paiement avec Stripe Payment Method (sécurisé)
        /// </summary>
        public async Task<PaymentResultViewModel> ProcessPaymentAsync(string paymentMethodId, long amountInCents, string email, string description)
        {
            try
            {
                var result = new PaymentResultViewModel();

                // Créer une PaymentIntent (méthode recommandée par Stripe)
                var intentOptions = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = "usd",
                    PaymentMethod = paymentMethodId,
                    ConfirmationMethod = "automatic",
                    Confirm = true,
                    ReturnUrl = "https://localhost:44306/Payment/Success",
                    ReceiptEmail = email,
                    Description = description,
                    Metadata = new Dictionary<string, string>
                    {
                        { "description", description }
                    }
                };

                var intentService = new PaymentIntentService();
                var paymentIntent = await intentService.CreateAsync(intentOptions);

                result.Success = paymentIntent.Status == "succeeded";
                result.TransactionId = paymentIntent.Id;
                result.Message = paymentIntent.Status == "succeeded" 
                    ? "Payment successful" 
                    : $"Payment status: {paymentIntent.Status}";

                _logger.LogInformation($"PaymentIntent processed: {paymentIntent.Id}, Status: {paymentIntent.Status}, Amount: {paymentIntent.Amount}");

                return result;
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error: {ex.Message}");
                return new PaymentResultViewModel
                {
                    Success = false,
                    Message = $"Payment failed: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing payment: {ex.Message}");
                return new PaymentResultViewModel
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Créer une session Stripe Checkout
        /// </summary>
        public async Task<Session> CreateCheckoutSessionAsync(int orderId, float amount, string customerEmail)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmountDecimal = (decimal)(amount * 100),
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"Order #{orderId}",
                                    Description = "GestionArticles Order"
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = $"https://localhost:44306/Payment/Success?orderId={orderId}",
                    CancelUrl = $"https://localhost:44306/Payment/Cancel?orderId={orderId}",
                    CustomerEmail = customerEmail,
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                _logger.LogInformation($"Checkout session created: {session.Id}");

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating checkout session: {ex.Message}");
                throw;
            }
        }
    }
}
