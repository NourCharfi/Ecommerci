using GestionArticles.Models.Orders;
using GestionArticles.Models.Notifications;
using GestionArticles.Models.Audit;
using GestionArticles.Models.Promotions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestionArticles.Models
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Panier> Paniers { get; set; }
        public DbSet<Commande> Commandes { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Favorite> Favorites { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        // ✅ NOUVEAU: Audit Logs
        public DbSet<AuditLog> AuditLogs { get; set; }

        // ✅ NOUVEAU: Discounts
        public DbSet<Discount> Discounts { get; set; }

        public DbSet<SearchHistory> SearchHistories { get; set; }
    }


}
