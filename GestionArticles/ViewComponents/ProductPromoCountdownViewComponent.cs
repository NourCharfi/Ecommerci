using GestionArticles.Models;
using GestionArticles.Models.Repositories;
using GestionArticles.Services;
using GestionArticles.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionArticles.ViewComponents
{
    public class ProductPromoCountdownViewComponent : ViewComponent
    {
        private readonly IDiscountService _discountService;

        public ProductPromoCountdownViewComponent(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        public IViewComponentResult Invoke(Product product)
        {
            var discount = _discountService.GetApplicableDiscount(product);
            
            if (discount == null || DateTime.Now > discount.EndDate)
                return Content("");

            var timeRemaining = discount.EndDate - DateTime.Now;
            var daysRemaining = timeRemaining.Days;
            var hoursRemaining = timeRemaining.Hours;
            var minutesRemaining = timeRemaining.Minutes;

            string timeText;
            string badgeClass;

            if (daysRemaining > 0)
            {
                timeText = $"Offre expire dans {daysRemaining}j {hoursRemaining}h";
                badgeClass = daysRemaining > 3 ? "badge-info" : "badge-warning";
            }
            else if (hoursRemaining > 0)
            {
                timeText = $"Offre expire dans {hoursRemaining}h {minutesRemaining}m";
                badgeClass = "badge-danger";
            }
            else
            {
                timeText = $"Expire dans {minutesRemaining}m";
                badgeClass = "badge-danger";
            }

            var model = new
            {
                TimeText = timeText,
                BadgeClass = badgeClass,
                DaysRemaining = daysRemaining
            };

            return View(model);
        }
    }
}
