using System.ComponentModel.DataAnnotations;

namespace GestionArticles.ViewModels.Roles
{
    public class EditRoleViewModel
    {
        public EditRoleViewModel()
        {
            Users = new List<string>();
            Id = string.Empty;
            RoleName = string.Empty;
        }
        [Required]
        public string Id { get; set; }
        [Required(ErrorMessage = "Role Name is required")]
        public string RoleName { get; set; }
        public List<string> Users { get; set; }
    }
}
