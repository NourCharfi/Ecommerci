using GestionArticles.Models;

namespace GestionArticles.Models.Repositories
{
    public interface IFavoriteRepository
    {
        bool IsFavorite(string userId, int productId);
        void AddFavorite(string userId, int productId);
        void RemoveFavorite(string userId, int productId);
        IList<Product> GetFavoritesByUser(string userId);
    }
}
