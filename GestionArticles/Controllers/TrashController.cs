using GestionArticles.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GestionArticles.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class TrashController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<TrashController> _logger;

        public TrashController(IProductRepository productRepository, UserManager<IdentityUser> userManager, ILogger<TrashController> logger)
        {
            _productRepository = productRepository;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Afficher la corbeille
        /// </summary>
        public IActionResult Index()
        {
            var deletedProducts = _productRepository.GetDeleted();
            return View(deletedProducts);
        }

        /// <summary>
        /// Restaurer un produit
        /// </summary>
        [HttpPost]
        public IActionResult Restore(int id)
        {
            var product = _productRepository.GetByIdIncludeDeleted(id);
            if (product == null || !product.IsDeleted)
                return NotFound();

            _productRepository.Restore(id);
            _logger.LogInformation($"? Produit restauré: {product.Name} par {_userManager.GetUserAsync(User).Result?.Email}");
            TempData["SuccessMessage"] = $"? {product.Name} a été restauré";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Supprimer définitivement
        /// </summary>
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var product = _productRepository.GetByIdIncludeDeleted(id);
            if (product == null || !product.IsDeleted)
                return NotFound();

            _productRepository.PermanentDelete(id);
            _logger.LogWarning($"??? Produit supprimé définitivement: {product.Name} par {_userManager.GetUserAsync(User).Result?.Email}");
            TempData["SuccessMessage"] = $"??? {product.Name} a été supprimé définitivement";

            return RedirectToAction("Index");
        }
    }
}
