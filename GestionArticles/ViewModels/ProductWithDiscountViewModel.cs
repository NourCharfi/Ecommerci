using GestionArticles.Models;
using GestionArticles.Models.Promotions;

namespace GestionArticles.ViewModels
{
    /// <summary>
    /// ViewModel pour afficher un produit avec sa réduction
    /// </summary>
    public class ProductWithDiscountViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public float OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public int DiscountPercentage { get; set; }
        public bool HasDiscount { get; set; }
        public string? DiscountName { get; set; }
        public Models.Promotions.DiscountType? DiscountType { get; set; }
        public Models.Promotions.Discount? Discount { get; set; }
        public string Image { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int QteStock { get; set; }
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Crée un ViewModel à partir d'un produit
        /// </summary>
        public static ProductWithDiscountViewModel FromProduct(
            Product product,
            Models.Promotions.Discount? discount = null)
        {
            decimal originalPrice = (decimal)product.Price;
            decimal discountedPrice = originalPrice;
            decimal discountAmount = 0;
            int discountPercentage = 0;
            bool hasDiscount = discount != null;

            if (discount != null)
            {
                if (discount.Type == Models.Promotions.DiscountType.Percentage)
                {
                    discountPercentage = (int)discount.Value;
                    discountAmount = (originalPrice * discount.Value) / 100m;
                    discountedPrice = originalPrice - discountAmount;
                }
                else if (discount.Type == Models.Promotions.DiscountType.FreeShipping)
                {
                    discountedPrice = originalPrice;
                    discountAmount = 0;
                }
            }

            return new ProductWithDiscountViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                OriginalPrice = product.Price,
                DiscountedPrice = discountedPrice,
                DiscountAmount = discountAmount,
                DiscountPercentage = discountPercentage,
                HasDiscount = hasDiscount,
                DiscountName = discount?.Name,
                DiscountType = discount?.Type,
                Discount = discount,
                Image = product.Image,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName,
                QteStock = product.QteStock,
                IsDeleted = product.IsDeleted
            };
        }
    }
}
