using GestionArticles.Models;
using GestionArticles.Models.Repositories;
using GestionArticles.Models.Audit;
using GestionArticles.Services;
using GestionArticles.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GestionArticles.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository ProductRepository;
        private readonly ICategorieRepository CategRepository;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IFavoriteRepository favoriteRepository;
        private readonly UserManager<IdentityUser> userManager;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogRepository _auditRepository;
        private readonly ILogger<ProductController> _logger;
        private readonly IDiscountService _discountService;
        private readonly ISearchHistoryRepository _searchHistoryRepository;

        public ProductController(IProductRepository prodRepository, ICategorieRepository categRepository,
           IWebHostEnvironment hostingEnvironment, IFavoriteRepository favoriteRepository, UserManager<IdentityUser> userManager,
           INotificationService notificationService, IAuditLogRepository auditRepository, ILogger<ProductController> logger,
           IDiscountService discountService, ISearchHistoryRepository searchHistoryRepository)
        {
            ProductRepository = prodRepository;
            CategRepository = categRepository;
            this.hostingEnvironment = hostingEnvironment;
            this.favoriteRepository = favoriteRepository;
            this.userManager = userManager;
            _notificationService = notificationService;
            _auditRepository = auditRepository;
            _logger = logger;
            _discountService = discountService;
            _searchHistoryRepository = searchHistoryRepository;
        }

        // GET: ProductController
        // Show all products
        [AllowAnonymous]
        public IActionResult Index(int? categoryId, int page = 1)
        {

            int pageSize = 4; // Nombre de produits par page
            var categories = CategRepository.GetAll();
            // Passer les catégories à la vue
            ViewData["Categories"] = categories;
            // Récupérer les produits en fonction de categoryId, s'il est spécifié
            IQueryable<Product> productsQuery = ProductRepository.GetAllProducts();
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);
            }
            // Pagination
            var totalProducts = productsQuery.Count();
            var products = productsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.CategoryId = categoryId; // Passer categoryId à la vue

            // Favorites for current user
            var favIds = new HashSet<int>();
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var uid = userManager.GetUserId(User);
                var favProducts = favoriteRepository.GetFavoritesByUser(uid);
                favIds = new HashSet<int>(favProducts.Select(p => p.ProductId));
            }
            ViewBag.FavIds = favIds;

            // Prix remisés pour affichage rapide
            ViewBag.DiscountedPrices = products.ToDictionary(
                p => p.ProductId,
                p => (float)_discountService.CalculateDiscountedPrice(p, (decimal)p.Price)
            );

            return View(products);
        }

        // Admin product management view (datatable)
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult AdminIndex()
        {
            var products = ProductRepository.GetAll();
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            return View(products);
        }

        // GET: ProductController/Details/5
        // Show details of a specific product
        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            var product = ProductRepository.GetById(id);  // ✅ use GetById
            if (product == null)
                return NotFound();

            bool isFavorite = false;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var uid = userManager.GetUserId(User);
                isFavorite = favoriteRepository.IsFavorite(uid, id);
            }
            ViewBag.IsFavorite = isFavorite;

            // Prix remisé et économie
            var discounted = _discountService.CalculateDiscountedPrice(product, (decimal)product.Price);
            ViewBag.DiscountedPrice = (float)discounted;
            ViewBag.Savings = product.Price - (float)discounted;

            return View(product);
        }

        // GET: ProductController/Create
        // Display the create product form
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult Create()
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            return View();
        }

        // POST: ProductController/Create
        // Handle the form submission for creating a new product
        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;

                // Check if user selected an image
                if (model.ImagePath != null)
                {
                    // Path to wwwroot/images folder
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");

                    // Make sure file name is unique
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Copy file to wwwroot/images
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ImagePath.CopyTo(fileStream);
                    }
                }

                // Create new Product object
                var user = await userManager.GetUserAsync(User);
                Product newProduct = new Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    QteStock = model.QteStock,
                    CategoryId = model.CategoryId,
                    Image = uniqueFileName,
                    Category = model.Category,
                    CreatedBy = user?.Email ?? "Admin",    // ✅ TRACK WHO CREATED
                    CreatedAt = DateTime.Now
                };

                // Add product to repository
                ProductRepository.Add(newProduct);

                // ✅ LOGGER: Enregistrer la création
                var auditLog = new AuditLog
                {
                    UserId = user?.Id,
                    UserEmail = user?.Email ?? "Inconnu",
                    ActionType = AuditActionType.Create,
                    EntityType = AuditEntityType.Product,
                    EntityId = newProduct.ProductId,
                    EntityName = newProduct.Name,
                    NewValues = JsonSerializer.Serialize(new { newProduct.Name, newProduct.Price, newProduct.QteStock }),
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };
                _auditRepository.LogAction(auditLog);

                _notificationService.NotifyNewProduct(newProduct.ProductId);
                _logger.LogInformation($"✅ Nouveau produit créé par {user?.Email}: {newProduct.Name}");

                TempData["SuccessMessage"] = "✅ Produit créé avec succès!";

                return RedirectToAction("Details", new { id = newProduct.ProductId });
            }

            ViewBag.CategoryId = new SelectList(
                CategRepository.GetAll(),
                "CategoryId",
                "CategoryName",
                model.CategoryId);

            return View(model);
        }

        // GET: ProductController/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult Edit(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            Product product = ProductRepository.GetById(id);
            var productEditViewModel = new GestionArticles.ViewModels.Products.EditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                QteStock = product.QteStock,
                CategoryId = product.CategoryId,
                ExistingImagePath = product.Image ?? string.Empty,
                Category = product.Category ?? new Category { CategoryName = string.Empty, Products = new List<Product>() },
                ImagePath = null!
            };
            return View(productEditViewModel);
        }
        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> Edit(GestionArticles.ViewModels.Products.EditViewModel model)
        {
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            if (model.ImagePath == null && !string.IsNullOrWhiteSpace(model.ExistingImagePath))
            {
                var keys = ModelState.Keys.Where(k => k.EndsWith("ImagePath", StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var k in keys) ModelState.Remove(k);
            }

            if (ModelState.IsValid)
            {
                Product product = ProductRepository.GetById(model.ProductId);
                var oldValues = JsonSerializer.Serialize(new { product.Name, product.Price, product.QteStock });

                product.Name = model.Name;
                product.Price = model.Price;
                product.QteStock = model.QteStock;
                product.CategoryId = model.CategoryId;

                if (model.ImagePath != null)
                {
                    if (model.ExistingImagePath != null)
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingImagePath);
                        System.IO.File.Delete(filePath);
                    }
                    product.Image = ProcessUploadedFile(model);
                }

                // ✅ TRACK WHO MODIFIED
                var user = await userManager.GetUserAsync(User);
                product.ModifiedBy = user?.Email;
                product.ModifiedAt = DateTime.Now;

                Product updatedProduct = ProductRepository.Update(product);

                // ✅ LOGGER: Enregistrer la modification
                if (updatedProduct != null)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = user?.Id,
                        UserEmail = user?.Email ?? "Inconnu",
                        ActionType = AuditActionType.Update,
                        EntityType = AuditEntityType.Product,
                        EntityId = product.ProductId,
                        EntityName = product.Name,
                        OldValues = oldValues,
                        NewValues = JsonSerializer.Serialize(new { product.Name, product.Price, product.QteStock }),
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                    };
                    _auditRepository.LogAction(auditLog);

                    _logger.LogInformation($"✏️ Produit modifié par {user?.Email}: {product.Name}");
                    TempData["SuccessMessage"] = "✅ Produit modifié avec succès!";

                    return RedirectToAction("Details", new { id = product.ProductId });
                }
                else
                    return NotFound();
            }
            return View(model);
        }
        [NonAction]
        private string ProcessUploadedFile(GestionArticles.ViewModels.Products.EditViewModel model)
        {
            string uniqueFileName = null;
            if (model.ImagePath != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImagePath.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        // GET: ProductController/Delete/5
        // Show confirmation page before deleting a product
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult Delete(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            var product = ProductRepository.GetById(id); // ✅ use GetById
            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: ProductController/Delete/5
        // Confirm and delete the product
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var product = ProductRepository.GetById(id);
            if (product != null)
            {
                var user = await userManager.GetUserAsync(User);
                var deletedBy = user?.Email ?? "Inconnu";
                
                // ✅ Sauvegarder les anciennes valeurs AVANT suppression
                var oldValues = JsonSerializer.Serialize(new { product.Name, product.Price, product.QteStock });
                
                ProductRepository.Delete(id, deletedBy);

                // ✅ LOGGER: Enregistrer la suppression avec oldValues
                var auditLog = new AuditLog
                {
                    UserId = user?.Id,
                    UserEmail = deletedBy,
                    ActionType = AuditActionType.Delete,
                    EntityType = AuditEntityType.Product,
                    EntityId = id,
                    EntityName = product.Name,
                    OldValues = oldValues,  // ✅ Ajouter oldValues
                    NewValues = null,  // ✅ NewValues est null pour Delete (pas de nouvelles valeurs)
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };
                _auditRepository.LogAction(auditLog);

                _logger.LogInformation($"🗑️ Produit envoyé en corbeille par {deletedBy}");
                TempData["SuccessMessage"] = "✅ Produit envoyé en corbeille.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public ActionResult Search(string val)
        {
            // Enregistrer l'historique des recherches
            try
            {
                var userId = userManager.GetUserId(User);
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (!string.IsNullOrWhiteSpace(val))
                {
                    _searchHistoryRepository.Add(new SearchHistory
                    {
                        UserId = userId,
                        Query = val.Trim(),
                        CreatedAt = DateTime.Now,
                        IpAddress = ip
                    });
                }
            }
            catch { /* ignore logging errors */ }

            var result = ProductRepository.FindByName(val);
            return View("Index", result);
        }
    }
}