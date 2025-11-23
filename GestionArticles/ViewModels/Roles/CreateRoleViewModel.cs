using System.ComponentModel.DataAnnotations;

namespace GestionArticles.ViewModels.Roles
{
    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; }
    }
}