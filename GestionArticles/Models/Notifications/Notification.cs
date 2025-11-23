namespace GestionArticles.Models.Notifications
{
    /// <summary>
    /// Type de notification
    /// </summary>
    public enum NotificationType
    {
        // Notifications Client
        OrderCreated = 0,              // Commande créée
        OrderConfirmed = 1,            // Commande confirmée/payée
        OrderShipping = 2,             // Commande en expédition
        OrderDelivered = 3,            // Commande livrée
        PaymentSuccessful = 4,         // Paiement réussi
        PaymentFailed = 5,             // Paiement échoué
        NewProductAdded = 6,           // Nouveau produit ajouté
        ProductRestock = 7,            // Produit réapprovisionné

        // Notifications Admin
        StockLow = 100,                // Stock faible
        StockRupture = 101,            // Rupture de stock
        NewUser = 102,                 // Nouvel utilisateur inscrit
        HighValueOrder = 103,          // Commande de valeur élevée
        PaymentReceived = 104          // Paiement reçu
    }

    /// <summary>
    /// Modèle de notification
    /// </summary>
    public class Notification
    {
        public int Id { get; set; }
        public string? UserId { get; set; }              // ID de l'utilisateur (null si admin)
        public NotificationType Type { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? Icon { get; set; }                // Font Awesome icon (fa-*)
        public string? Color { get; set; }               // Bootstrap color class (primary, success, warning, danger, etc)
        public int? OrderId { get; set; }               // ID de la commande (si applicable)
        public int? ProductId { get; set; }             // ID du produit (si applicable)
        public string? ActionUrl { get; set; }           // URL d'action
        public bool IsRead { get; set; }               // Lue/non-lue
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsAdmin { get; set; }              // True si notification admin
    }
}
