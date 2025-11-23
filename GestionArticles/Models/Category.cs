using System.ComponentModel.DataAnnotations;

namespace GestionArticles.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        [Required]
        [Display(Name = "Nom")]
        public required string CategoryName { get; set; }
        // Optional image filename (stored under wwwroot/images/categories)
        [Display(Name = "Image de la catégorie")]
        public string? Image { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
