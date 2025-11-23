using System.ComponentModel.DataAnnotations;

namespace GestionArticles.ViewModels.Auth
{
    public class EditProfileViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Nom d'utilisateur (facultatif)")]
        [StringLength(50, ErrorMessage = "Le nom d'utilisateur ne doit pas dépasser 50 caractères.")]
        public string? UserName { get; set; }

        [Phone]
        [Display(Name = "Téléphone (facultatif)")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe actuel")]
        public string? CurrentPassword { get; set; } // facultatif

        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe (facultatif)")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        public string? NewPassword { get; set; } // facultatif

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le nouveau mot de passe (facultatif)")]
        [Compare("NewPassword", ErrorMessage = "Le mot de passe et la confirmation ne correspondent pas.")]
        public string? ConfirmNewPassword { get; set; } // facultatif
    }
}