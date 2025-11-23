using System.ComponentModel.DataAnnotations;

namespace GestionArticles.ViewModels.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'identifiant est requis.")]
        [Display(Name = "Email ou nom d'utilisateur")]
        public string Email { get; set; } = string.Empty; // peut contenir email ou username

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Se souvenir de moi")]
        public bool RememberMe { get; set; }
    }
}
