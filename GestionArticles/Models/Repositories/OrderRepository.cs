using GestionArticles.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace GestionArticles.Models.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        readonly AppDbContext context;
        public OrderRepository(AppDbContext context)
        {
            this.context = context;
        }
        public void Add(Order o)
        {
            context.Orders.Add(o);
            context.SaveChanges();
        }
        public Order GetById(int id)
        {
            return context.Orders
            .Include(o => o.Items)
            .FirstOrDefault(o => o.Id == id);
        }
        public void Update(Order o)
        {
            var existing = context.Orders.Find(o.Id);
            if (existing != null)
            {
                existing.Status = o.Status;
                context.SaveChanges();
            }
        }
        public IList<Order> GetAll()
        {
            return context.Orders.Include(o => o.Items).OrderByDescending(o => o.OrderDate).ToList();
        }
        public IList<Order> GetByUserId(string userId)
        {
            return context.Orders.Where(o => o.UserId == userId).Include(o => o.Items).OrderByDescending(o => o.OrderDate).ToList();
        }
    }
}
