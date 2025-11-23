using GestionArticles.Models.Orders;

namespace GestionArticles.Models.Repositories
{
    public interface IOrderRepository
    {
        Order GetById(int Id);
        void Add(Order o);
        void Update(Order o);
        IList<Order> GetAll();
        IList<Order> GetByUserId(string userId);
    }


}
