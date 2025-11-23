namespace GestionArticles.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Stats générales
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }

        // Produits
        public int ProductsLowStock { get; set; }
        public int ProductsOutOfStock { get; set; }
        public int ProductsDeleted { get; set; }

        // Commandes
        public int OrdersPending { get; set; }
        public int OrdersShipping { get; set; }
        public int OrdersDelivered { get; set; }

        // Activités récentes
        public List<string> RecentActivities { get; set; }
        public List<OrderSummary> RecentOrders { get; set; }

        // Audit
        public List<AuditSummary> RecentAuditLogs { get; set; }
    }

    public class OrderSummary
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
    }

    public class AuditSummary
    {
        public string UserEmail { get; set; }
        public string Action { get; set; }
        public string Entity { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ManagerDashboardViewModel
    {
        // Stats pour manager (produits + commandes)
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public int ProductsLowStock { get; set; }
        public int OrdersPending { get; set; }

        public List<OrderSummary> RecentOrders { get; set; }
    }
}
