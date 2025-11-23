using GestionArticles.Models;
using GestionArticles.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace GestionArticles.Controllers
{

    [Authorize(Roles = "Admin,Manager")]


    public class CategoryController : Controller
    {
        private readonly ICategorieRepository categorieRepository;
        private readonly IWebHostEnvironment env;

        public CategoryController(ICategorieRepository categorieRepository, IWebHostEnvironment env)
        {
            this.categorieRepository = categorieRepository;
            this.env = env;
        }

        // GET: CategoryController

        [AllowAnonymous]

        public ActionResult Index()
        {
            var categories = categorieRepository.GetAll();   // 🔹 like your employees example
            ViewData["Categories"] = categories;
            return View("Index-Simple", categories);
        }

        // GET: CategoryController/Details/5
        public ActionResult Details(int id)
        {
            var categories = categorieRepository.GetAll();
            ViewData["Categories"] = categories;
            var category = categorieRepository.GetById(id); // 🔹 similar to employeeRepository.FindByID
            if (category == null)
                return NotFound();

            return View(category);
        }

        // GET: CategoryController/Create
        public ActionResult Create()
        {
            var categories = categorieRepository.GetAll();
            ViewData["Categories"] = categories;
            return View();
        }

        // POST: CategoryController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category, IFormFile? imageFile)
        {
            // Log: début de la méthode
            Console.WriteLine($"[LOG] POST Create called. CategoryName: {category?.CategoryName}");
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploads = Path.Combine(env.WebRootPath, "images", "categories");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploads, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }
                    category.Image = fileName;
                }

                categorieRepository.Add(category);
                Console.WriteLine($"[LOG] Category added: {category.CategoryName}");
                return RedirectToAction(nameof(Index));
            }
            Console.WriteLine("[LOG] ModelState is not valid.");
            return View(category);
        }

        // GET: CategoryController/Edit/5
        public ActionResult Edit(int id)
        {
            var categories = categorieRepository.GetAll();
            ViewData["Categories"] = categories;
            var category = categorieRepository.GetById(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: CategoryController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Category newCategory, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                newCategory.CategoryId = id;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploads = Path.Combine(env.WebRootPath, "images", "categories");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploads, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }
                    newCategory.Image = fileName;
                }
                else
                {
                    // Preserve existing image when no new file uploaded
                    var existing = categorieRepository.GetById(id);
                    newCategory.Image = existing?.Image;
                }

                categorieRepository.Update(newCategory);
                return RedirectToAction(nameof(Index));
            }

            return View(newCategory);
        }


        // GET: CategoryController/Delete/5
        public ActionResult Delete(int id)
        {
            var categories = categorieRepository.GetAll();
            ViewData["Categories"] = categories;
            var category = categorieRepository.GetById(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: CategoryController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            categorieRepository.Delete(id);   // 🔹 Delete
            return RedirectToAction(nameof(Index));
        }
    }
}

