using GestionArticles.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GestionArticles.Models.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly AppDbContext _context;
        public FavoriteRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool IsFavorite(string userId, int productId)
        {
            return _context.Favorites
                .AsNoTracking()
                .Any(f => f.UserId == userId && f.ProductId == productId);
        }

        public void AddFavorite(string userId, int productId)
        {
            var fav = new Favorite { UserId = userId, ProductId = productId };
            _context.Favorites.Add(fav);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                // If unique constraint prevents duplicate insert, ignore
            }
        }

        public void RemoveFavorite(string userId, int productId)
        {
            var affected = _context.Favorites
                .Where(f => f.UserId == userId && f.ProductId == productId)
                .ExecuteDelete();
            // affected == 0 means row already removed; treat as success
        }

        public IList<Product> GetFavoritesByUser(string userId)
        {
            // Get distinct product ids to avoid duplicates when duplicate Favorite rows exist
            var productIds = _context.Favorites
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Select(f => f.ProductId)
                .Distinct()
                .ToList();

            if (!productIds.Any()) return new List<Product>();

            return _context.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.ProductId))
                .Include(p => p.Category)
                .ToList();
        }
    }
}
