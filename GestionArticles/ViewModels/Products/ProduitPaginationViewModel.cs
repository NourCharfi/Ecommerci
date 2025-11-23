using GestionArticles.Models;

namespace GestionArticles.ViewModels.Products
{
    public class ProduitPaginationViewModel
    {
        public List<Product> Products { get; set; }
        public int PageActuelle { get; set; }
        public int TotalPages { get; set; }
    }
}
