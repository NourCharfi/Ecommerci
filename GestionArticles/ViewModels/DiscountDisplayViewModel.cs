namespace GestionArticles.ViewModels
{
    public class DiscountDisplayViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }  // "Réduction %", "Livraison gratuite", "Acheter X obtenir Y"
        public decimal Value { get; set; }
        public int? BuyXGetYFreeQuantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int DaysRemaining { get; set; }
        public string TimeRemaining { get; set; }  // "3 jours", "12 heures", etc.
        public bool IsExpired { get; set; }
        
        // Pour l'affichage du badge
        public string BadgeText { get; set; }
        public string BadgeClass { get; set; }  // "badge-success", "badge-warning", etc.
    }
}
