using GestionArticles.Models.Repositories;
using GestionArticles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Diagnostics;

namespace GestionArticles.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class StockController : Controller
    {
        private readonly IProductRepository productRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<StockController> _logger;

        public StockController(IProductRepository productRepository, INotificationService notificationService, ILogger<StockController> logger)
        {
            this.productRepository = productRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        // GET: Stock/Index
        public ActionResult Index()
        {
            // Récupère tous les produits et les envoie à la vue
            var products = productRepository.GetAll();
            return View(products);
        }

        // GET: Stock/Details/5
        public ActionResult Details(int id)
        {
            var product = productRepository.GetById(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // GET: Stock/Edit/5
        public ActionResult Edit(int id)
        {
            var product = productRepository.GetById(id);
            if (product == null) return NotFound();

            // Pass categories to the view via ViewData for a dropdown if needed
            try
            {
                var categRepo = HttpContext.RequestServices.GetService(typeof(GestionArticles.Models.Repositories.ICategorieRepository)) as GestionArticles.Models.Repositories.ICategorieRepository;
                if (categRepo != null)
                {
                    ViewData["Categories"] = categRepo.GetAll();
                }
            }
            catch { }

            return View(product);
        }

        // POST: Stock/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, GestionArticles.Models.Product model)
        {
            if (id != model.ProductId) return BadRequest();

            // Load existing product to preserve fields that are not edited (like Image)
            var existing = productRepository.GetById(id);
            if (existing == null) return NotFound();

            // Store old stock for comparison
            int oldStock = existing.QteStock;

            // If no image was submitted, preserve the existing image and clear ModelState errors for Image
            if (string.IsNullOrWhiteSpace(model.Image) && !string.IsNullOrWhiteSpace(existing.Image))
            {
                model.Image = existing.Image;
                var keys = ModelState.Keys.Where(k => k.EndsWith("Image", System.StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var k in keys) ModelState.Remove(k);
            }

            // If CategoryId was not provided (0), preserve existing CategoryId and remove ModelState entries related to Category
            if (model.CategoryId == 0 && existing.CategoryId > 0)
            {
                model.CategoryId = existing.CategoryId;
                var catKeys = ModelState.Keys.Where(k => k.Equals("Category", System.StringComparison.OrdinalIgnoreCase) || k.StartsWith("Category.", System.StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var k in catKeys) ModelState.Remove(k);
            }

            if (!ModelState.IsValid)
            {
                // collect ModelState errors for debugging
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                if (!errors.Any())
                {
                    // sometimes errors have exception info; include those
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.Exception?.Message).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                }
                TempData["ModelErrors"] = string.Join(" | ", errors);
                Debug.WriteLine("Stock/Edit modelstate errors: " + (TempData["ModelErrors"] ?? "(none)"));

                // reload categories
                try
                {
                    var categRepo = HttpContext.RequestServices.GetService(typeof(GestionArticles.Models.Repositories.ICategorieRepository)) as GestionArticles.Models.Repositories.ICategorieRepository;
                    if (categRepo != null)
                    {
                        ViewData["Categories"] = categRepo.GetAll();
                    }
                }
                catch { }
                return View(model);
            }

            // Apply updates to the existing entity to avoid binding surprises
            existing.Name = model.Name;
            existing.Price = model.Price;
            existing.QteStock = model.QteStock;
            existing.CategoryId = model.CategoryId;
            // preserve existing.Image already set on existing

            // save via repository
            var updated = productRepository.Update(existing);
            if (updated == null) return NotFound();

            // ✅ AJOUTER: Notifier les admins des changements de stock
            if (updated.QteStock == 0 && oldStock > 0)
            {
                _notificationService.NotifyAdminStockRupture(updated.ProductId);
                _logger.LogWarning($"🔴 RUPTURE DE STOCK: {updated.Name} (ID: {updated.ProductId})");
                TempData["WarningMessage"] = $"⚠️ {updated.Name} est en RUPTURE DE STOCK!";
            }
            else if (updated.QteStock > 0 && updated.QteStock <= 20 && oldStock > 20)
            {
                _notificationService.NotifyAdminStockLow(updated.ProductId, updated.QteStock);
                _logger.LogWarning($"🟡 STOCK FAIBLE: {updated.Name} - Quantité: {updated.QteStock}");
                TempData["WarningMessage"] = $"⚠️ {updated.Name} a un stock faible: {updated.QteStock} unités";
            }
            else if (updated.QteStock > 20 && oldStock <= 20)
            {
                _logger.LogInformation($"✅ RÉAPPROVISIONNÉ: {updated.Name} - Quantité: {updated.QteStock}");
                TempData["SuccessMessage"] = $"✅ {updated.Name} a été réapprovisionné: {updated.QteStock} unités";
            }
            else
            {
                TempData["SuccessMessage"] = "✅ Modifications enregistrées avec succès.";
            }

            return RedirectToAction("Details", new { id = updated.ProductId });
        }

        // GET: HomeController1/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HomeController1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController1/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HomeController1/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
