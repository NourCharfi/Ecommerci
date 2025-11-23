using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GestionArticles.ViewModels.Products
{
    public class EditViewModel
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 5)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Prix en dinar :")]
        public float Price { get; set; }

        [Required]
        [Display(Name = "Quantité en unité :")]
        public int QteStock { get; set; }

        public int CategoryId { get; set; }
        public GestionArticles.Models.Category? Category { get; set; }

        // Existing image path may be null when editing
        public string? ExistingImagePath { get; set; }

        // New image is optional during edit
        public IFormFile? ImagePath { get; set; }
    }
}
