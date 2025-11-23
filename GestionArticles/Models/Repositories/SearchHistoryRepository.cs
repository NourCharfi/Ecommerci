using GestionArticles.Models;

namespace GestionArticles.Models.Repositories
{
    public interface ISearchHistoryRepository
    {
        void Add(SearchHistory entry);
        IList<SearchHistory> GetByUser(string userId, int count = 20);
        IList<SearchHistory> GetAll(int count = 100);
    }

    public class SearchHistoryRepository : ISearchHistoryRepository
    {
        private readonly AppDbContext _context;
        public SearchHistoryRepository(AppDbContext context) { _context = context; }

        public void Add(SearchHistory entry)
        {
            _context.Add(entry);
            _context.SaveChanges();
        }

        public IList<SearchHistory> GetByUser(string userId, int count = 20)
        {
            return _context.Set<SearchHistory>()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(count)
                .ToList();
        }

        public IList<SearchHistory> GetAll(int count = 100)
        {
            return _context.Set<SearchHistory>()
                .OrderByDescending(s => s.CreatedAt)
                .Take(count)
                .ToList();
        }
    }
}
