using System.ComponentModel.DataAnnotations;

namespace GestionArticles.Models.Orders
{
    public enum OrderStatus
    {
        [Display(Name = "Traitement")] Processing = 0,
        [Display(Name = "Préparation")] Preparing = 1,
        [Display(Name = "Expédition")] Shipping = 2,
        [Display(Name = "Livrée")] Delivered = 3,
        [Display(Name = "Payée")] Paid = 4,        // Après paiement
        [Display(Name = "Confirmée")] Confirmed = 5 // Validation interne finale
    }
}