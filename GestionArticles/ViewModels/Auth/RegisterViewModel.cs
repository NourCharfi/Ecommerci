using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GestionArticles.ViewModels.Auth
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
        [Display(Name = "Nom d'utilisateur")]
        [StringLength(50, ErrorMessage = "Le nom d'utilisateur ne doit pas dépasser 50 caractères.")]
        [MinLength(3, ErrorMessage = "Le nom d'utilisateur doit contenir au moins 3 caractères.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le numéro de téléphone est requis.")]
        [Display(Name = "Téléphone")]
        [RegularExpression(@"^[0-9+\s]{6,20}$", ErrorMessage = "Numéro de téléphone invalide (chiffres, + et espaces seulement).")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'adresse est requise.")]
        [StringLength(200, ErrorMessage = "L'adresse ne doit pas dépasser 200 caractères.")]
        [Display(Name = "Adresse")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare("Password", ErrorMessage = "Le mot de passe et la confirmation ne correspondent pas.")]
        [Required(ErrorMessage = "La confirmation du mot de passe est requise.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}