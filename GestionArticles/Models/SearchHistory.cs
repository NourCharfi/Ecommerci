using System.ComponentModel.DataAnnotations;

namespace GestionArticles.Models
{
    public class SearchHistory
    {
        public int Id { get; set; }
        [MaxLength(450)]
        public string? UserId { get; set; }
        [Required]
        [MaxLength(256)]
        public string Query { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? IpAddress { get; set; }
    }
}
