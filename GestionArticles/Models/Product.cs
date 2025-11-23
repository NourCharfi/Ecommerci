using System.ComponentModel.DataAnnotations;

namespace GestionArticles.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 5)]
        public required string Name { get; set; }
        [Required]
        [Display(Name = "Prix en dinar :")]
        public float Price { get; set; }
        [Required]
        [Display(Name = "Quantité en unité :")]
        public int QteStock { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required]
        [Display(Name = "Image :")]
        public required string Image { get; set; }

        // ✅ SOFT DELETE
        public bool IsDeleted { get; set; } = false;
        public string? DeletedBy { get; set; }        // Qui a supprimé
        public DateTime? DeletedAt { get; set; }      // Quand supprimé

        // ✅ TRACKING UTILISATEURS
        public string? CreatedBy { get; set; }        // Qui a créé
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }       // Qui a modifié
        public DateTime? ModifiedAt { get; set; }
    }
}
