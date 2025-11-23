using Stripe;

namespace GestionArticles.ViewModels.Payment
{
    public class PaymentViewModel
    {
        public int OrderId { get; set; }
        public float TotalAmount { get; set; }
        public string? CardNumber { get; set; }
        public string? CardExpiry { get; set; }
        public string? CardCVC { get; set; }
        public string? CardholderName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; } // Ajout pour paiement manuel
    }

    public class PaymentResultViewModel
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? TransactionId { get; set; }
        public int OrderId { get; set; }
    }
}
