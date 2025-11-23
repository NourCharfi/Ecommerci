using GestionArticles.Models.Promotions;

namespace GestionArticles.Models.Repositories
{
    public interface IDiscountRepository
    {
        void Add(Discount discount);
        IList<Discount> GetAll();
        IList<Discount> GetActive();
        Discount GetById(int id);
        void Update(Discount discount);
        void Delete(int id);
        IList<Discount> GetByProduct(int productId);
        IList<Discount> GetByCategory(int categoryId);
        Discount GetByPromoCode(string code);
    }

    public class DiscountRepository : IDiscountRepository
    {
        private readonly AppDbContext _context;

        public DiscountRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Discount discount)
        {
            _context.Discounts.Add(discount);
            _context.SaveChanges();
        }

        public IList<Discount> GetAll()
        {
            return _context.Discounts
                .OrderByDescending(d => d.CreatedAt)
                .ToList();
        }

        public IList<Discount> GetActive()
        {
            var now = DateTime.Now;
            return _context.Discounts
                .Where(d => d.IsActive && d.StartDate <= now && d.EndDate >= now)
                .ToList();
        }

        public Discount GetById(int id)
        {
            return _context.Discounts.Find(id);
        }

        public void Update(Discount discount)
        {
            _context.Discounts.Update(discount);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var discount = _context.Discounts.Find(id);
            if (discount != null)
            {
                _context.Discounts.Remove(discount);
                _context.SaveChanges();
            }
        }

        public IList<Discount> GetByProduct(int productId)
        {
            return _context.Discounts
                .Where(d => d.IsActive && (d.TargetProductId == productId || d.TargetProductId == null))
                .ToList();
        }

        public IList<Discount> GetByCategory(int categoryId)
        {
            return _context.Discounts
                .Where(d => d.IsActive && (d.TargetCategoryId == categoryId || d.TargetCategoryId == null))
                .ToList();
        }

        public Discount GetByPromoCode(string code)
        {
            return _context.Discounts
                .FirstOrDefault(d => d.PromoCode == code && d.IsActive && d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now);
        }
    }
}
