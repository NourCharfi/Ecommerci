using Microsoft.AspNetCore.Identity;

namespace GestionArticles.Models
{
    public class Favorite
    {
        public int FavoriteId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }

        // Navigation properties
        public IdentityUser? User { get; set; }
        public Product? Product { get; set; }
    }
}
