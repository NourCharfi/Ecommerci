using GestionArticles.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GestionArticles.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly IFavoriteRepository favoriteRepository;
        private readonly UserManager<IdentityUser> userManager;

        public FavoritesController(IFavoriteRepository favoriteRepository, UserManager<IdentityUser> userManager)
        {
            this.favoriteRepository = favoriteRepository;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            var user = userManager.GetUserAsync(User).Result;
            if (user == null) return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            var favs = favoriteRepository.GetFavoritesByUser(user.Id);
            return View(favs);
        }

        [HttpPost]
        public IActionResult Toggle(int id)
        {
            var user = userManager.GetUserAsync(User).Result;
            if (user == null) return Json(new { success = false, message = "Utilisateur non authentifié." });
            var isFav = favoriteRepository.IsFavorite(user.Id, id);
            if (isFav)
            {
                favoriteRepository.RemoveFavorite(user.Id, id);
                return Json(new { success = true, favorited = false, productId = id });
            }
            else
            {
                favoriteRepository.AddFavorite(user.Id, id);
                return Json(new { success = true, favorited = true, productId = id });
            }
        }
    }
}
