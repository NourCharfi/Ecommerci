using System;
using System.Collections.Generic;

namespace GestionArticles.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int AdminCount { get; set; }
        public int ManagerCount { get; set; }
        public int UserCount { get; set; }
        // Nombre de produits
        public int ProductCount { get; set; }
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new List<RecentOrderViewModel>();
    }

    public class RecentOrderViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string UserEmail { get; set; }
    }
}
