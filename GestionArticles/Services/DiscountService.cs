using GestionArticles.Models;
using GestionArticles.Models.Promotions;
using GestionArticles.Models.Repositories;

namespace GestionArticles.Services
{
    /// <summary>
    /// Service de gestion des promotions et réductions sur les produits
    /// </summary>
    public interface IDiscountService
    {
        /// <summary>
        /// Calcule le prix avec réduction pour un produit
        /// </summary>
        decimal CalculateDiscountedPrice(Product product, decimal originalPrice);

        /// <summary>
        /// Obtient la réduction applicable à un produit
        /// </summary>
        Discount? GetApplicableDiscount(Product product);

        /// <summary>
        /// Calcule le montant de la réduction
        /// </summary>
        decimal CalculateDiscountAmount(decimal price, Discount discount);

        /// <summary>
        /// Vérifie si une promotion est valide et applicable
        /// </summary>
        bool IsDiscountValid(Discount discount);

        /// <summary>
        /// Vérifie si un produit bénéficie de la livraison gratuite
        /// </summary>
        bool HasFreeShipping(Product product);

        /// <summary>
        /// Calcule le total d'une ligne de panier
        /// </summary>
        decimal CalculateRowTotal(Product product, int quantity, decimal unitPrice);
    }

    public class DiscountService : IDiscountService
    {
        private readonly IDiscountRepository _discountRepository;

        public DiscountService(IDiscountRepository discountRepository)
        {
            _discountRepository = discountRepository;
        }

        /// <summary>
        /// Calcule le prix avec réduction pour un produit
        /// </summary>
        public decimal CalculateDiscountedPrice(Product product, decimal originalPrice)
        {
            var discount = GetApplicableDiscount(product);
            if (discount == null) return originalPrice;

            // Pas de réduction pour Livraison gratuite (géré séparément)
            if (discount.Type == DiscountType.FreeShipping)
                return originalPrice;

            var discountAmount = CalculateDiscountAmount(originalPrice, discount);
            return Math.Max(0, originalPrice - discountAmount);
        }

        /// <summary>
        /// Calcule le total d'une ligne de panier
        /// </summary>
        public decimal CalculateRowTotal(Product product, int quantity, decimal unitPrice)
        {
            var discount = GetApplicableDiscount(product);
            if (discount == null)
                return unitPrice * quantity;

            switch (discount.Type)
            {
                case DiscountType.Percentage:
                    var discountedUnit = CalculateDiscountedPrice(product, unitPrice);
                    return discountedUnit * quantity;
                case DiscountType.FreeShipping:
                    // Le prix reste identique, la livraison sera gérée ailleurs
                    return unitPrice * quantity;
                default:
                    return unitPrice * quantity;
            }
        }

        /// <summary>
        /// Obtient la réduction applicable à un produit
        /// </summary>
        public Discount? GetApplicableDiscount(Product product)
        {
            var now = DateTime.Now;
            var discounts = _discountRepository.GetActive();

            // Chercher une réduction applicable
            return discounts.FirstOrDefault(d =>
                IsDiscountValid(d) &&
                d.StartDate <= now &&
                d.EndDate >= now &&
                (
                    // Réduction applicable à TOUS les produits
                    (d.TargetProductId == null && d.TargetCategoryId == null) ||
                    // Réduction sur ce produit spécifique
                    (d.TargetProductId == product.ProductId) ||
                    // Réduction sur cette catégorie
                    (d.TargetCategoryId == product.CategoryId)
                )
            );
        }

        /// <summary>
        /// Calcule le montant de la réduction
        /// </summary>
        public decimal CalculateDiscountAmount(decimal price, Discount discount)
        {
            return discount.Type switch
            {
                DiscountType.Percentage => (price * discount.Value) / 100m,
                DiscountType.FreeShipping => 0,
                _ => 0
            };
        }

        /// <summary>
        /// Vérifie si une promotion est valide et applicable
        /// </summary>
        public bool IsDiscountValid(Discount discount)
        {
            return discount != null && discount.IsActive && discount.StartDate <= discount.EndDate;
        }

        /// <summary>
        /// Vérifie si un produit bénéficie de la livraison gratuite
        /// </summary>
        public bool HasFreeShipping(Product product)
        {
            var discount = GetApplicableDiscount(product);
            return discount?.Type == DiscountType.FreeShipping;
        }
    }
}
