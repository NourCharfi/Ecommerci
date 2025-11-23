using Microsoft.AspNetCore.Identity;

namespace GestionArticles.Models.Orders
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public float TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        // personal info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        // payment / delivery
        public float DeliveryFee { get; set; }
        public string PaymentMethod { get; set; }
        // Lien avec l'utilisateur dans Identity
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        // Liste des articles de la commande
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        // Statut de la commande
        public OrderStatus Status { get; set; } = OrderStatus.Processing;
        
        // ✅ TRACKING
        public string? CreatedBy { get; set; }        // Qui a créé la commande (admin)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

}