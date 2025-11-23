using GestionArticles.Models.Repositories;
using GestionArticles.Models.Promotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionArticles.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DiscountController : Controller
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICategorieRepository _categoryRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuditLogRepository _auditRepository;
        private readonly ILogger<DiscountController> _logger;

        public DiscountController(
            IDiscountRepository discountRepository,
            IProductRepository productRepository,
            ICategorieRepository categoryRepository,
            UserManager<IdentityUser> userManager,
            IAuditLogRepository auditRepository,
            ILogger<DiscountController> logger)
        {
            _discountRepository = discountRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _auditRepository = auditRepository;
            _logger = logger;
        }

        /// <summary>
        /// Liste des soldes/promotions
        /// </summary>
        public IActionResult Index()
        {
            var discounts = _discountRepository.GetAll();
            return View(discounts);
        }

        /// <summary>
        /// Créer une nouvelle promo
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Products = new SelectList(_productRepository.GetAll(), "ProductId", "Name");
            ViewBag.Categories = new SelectList(_categoryRepository.GetAll(), "CategoryId", "CategoryName");
            return View();
        }

        /// <summary>
        /// Sauvegarder une nouvelle promo
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(Discount discount)
        {
            if (discount.StartDate >= discount.EndDate)
            {
                ModelState.AddModelError("", "La date début doit être avant la date fin");
            }

            if (string.IsNullOrEmpty(discount.Name))
            {
                ModelState.AddModelError("Name", "Le nom est requis");
            }

            if (discount.Value <= 0)
            {
                ModelState.AddModelError("Value", "La valeur doit être supérieure à 0");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                discount.CreatedBy = user?.Email ?? "Admin";
                discount.CreatedAt = DateTime.Now;
                discount.ModifiedBy = user?.Email ?? "Admin";  // Set ModifiedBy
                discount.ModifiedAt = DateTime.Now;  // Set ModifiedAt

                _discountRepository.Add(discount);

                _logger.LogInformation($"Promotion créée: {discount.Name} par {user?.Email}");
                TempData["SuccessMessage"] = "Promotion créée avec succès!";

                return RedirectToAction("Index");
            }

            ViewBag.Products = new SelectList(_productRepository.GetAll(), "ProductId", "Name", discount.TargetProductId);
            ViewBag.Categories = new SelectList(_categoryRepository.GetAll(), "CategoryId", "CategoryName", discount.TargetCategoryId);
            return View(discount);
        }

        /// <summary>
        /// Éditer une promo
        /// </summary>
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var discount = _discountRepository.GetById(id);
            if (discount == null)
                return NotFound();

            ViewBag.Products = new SelectList(_productRepository.GetAll(), "ProductId", "Name", discount.TargetProductId);
            ViewBag.Categories = new SelectList(_categoryRepository.GetAll(), "CategoryId", "CategoryName", discount.TargetCategoryId);
            return View(discount);
        }

        /// <summary>
        /// Sauvegarder les modifications
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(Discount discount)
        {
            var existing = _discountRepository.GetById(discount.Id);
            if (existing == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            existing.Name = discount.Name;
            existing.Description = discount.Description;
            existing.Type = discount.Type;
            existing.Value = discount.Value;
            existing.MinimumAmount = discount.MinimumAmount;
            existing.MinimumQuantity = discount.MinimumQuantity;
            existing.StartDate = discount.StartDate;
            existing.EndDate = discount.EndDate;
            existing.IsActive = discount.IsActive;
            existing.MaxUsageCount = discount.MaxUsageCount;
            existing.TargetProductId = discount.TargetProductId;
            existing.TargetCategoryId = discount.TargetCategoryId;
            existing.ModifiedBy = user?.Email;
            existing.ModifiedAt = DateTime.Now;

            _discountRepository.Update(existing);

            _logger.LogInformation($"Promotion modifiée: {discount.Name} par {user?.Email}");
            TempData["SuccessMessage"] = "Promotion modifiée!";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Supprimer une promo
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var discount = _discountRepository.GetById(id);
            if (discount == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            _discountRepository.Delete(id);

            _logger.LogWarning($"Promotion supprimée: {discount.Name} par {user?.Email}");
            TempData["SuccessMessage"] = "Promotion supprimée!";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Activer/Désactiver
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var discount = _discountRepository.GetById(id);
            if (discount == null)
                return NotFound();

            discount.IsActive = !discount.IsActive;
            _discountRepository.Update(discount);

            var status = discount.IsActive ? "Activée" : "Désactivée";
            TempData["SuccessMessage"] = $"Promotion {status}!";

            return RedirectToAction("Index");
        }
    }
}
