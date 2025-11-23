namespace GestionArticles.ViewModels.Panier
{
    public class CartItemViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public float Price { get; set; } // Prix unitaire original (DT)
        public float? DiscountedUnitPrice { get; set; } // Prix unitaire après réduction (Percentage uniquement)
        public float? RowTotal { get; set; } // Total ligne après application promo (BuyXGetY inclus)
        public float? Savings { get; set; } // Économie totale sur la ligne
        public int? FreeUnits { get; set; } // Unités gratuites (BuyXGetY)
        public string? DiscountType { get; set; } // Type de promotion appliquée
    }
}
