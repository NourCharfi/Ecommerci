namespace GestionArticles.ViewModels.Admin
{
    public class CreateUserViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Role { get; set; }
    }
}