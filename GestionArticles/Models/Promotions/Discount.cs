namespace GestionArticles.Models.Promotions
{
    public enum DiscountType
    {
        Percentage = 1,           // % réduction
        FreeShipping = 2          // Livraison gratuite
    }

    public class Discount
    {
        public int Id { get; set; }
        
        // Info basique
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DiscountType Type { get; set; }
        
        // Valeur selon Type:
        // - Percentage: % de réduction (ex: 20 = 20%)
        // - FreeShipping: non utilisé (toujours gratuit)
        public decimal Value { get; set; }
        
        // Conditions
        public decimal? MinimumAmount { get; set; }
        public int? MinimumQuantity { get; set; }
        
        // Périodes
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Code promo (optionnel)
        public string PromoCode { get; set; } = string.Empty;
        
        // Restrictions
        public int? MaxUsageCount { get; set; }
        public int CurrentUsageCount { get; set; } = 0;
        
        // Produits/Catégories ciblées
        public int? TargetProductId { get; set; }
        public int? TargetCategoryId { get; set; }
        
        // Tracking
        public string CreatedBy { get; set; } = "Admin";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string ModifiedBy { get; set; } = "System";
        public DateTime? ModifiedAt { get; set; }
    }
}
