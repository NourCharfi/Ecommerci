using GestionArticles.Models.Audit;

namespace GestionArticles.Models.Repositories
{
    public interface IAuditLogRepository
    {
        void LogAction(AuditLog log);
        IList<AuditLog> GetAll(int count = 100);
        IList<AuditLog> GetByUser(string userId, int count = 50);
        IList<AuditLog> GetByEntityType(AuditEntityType entityType, int count = 50);
        IList<AuditLog> GetByDateRange(DateTime from, DateTime to);
        IList<AuditLog> GetByDate(DateTime date);
    }

    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext _context;

        public AuditLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public void LogAction(AuditLog log)
        {
            if (log != null)
            {
                log.Timestamp = DateTime.Now;

                // Éviter toute insertion NULL si la migration n'est pas appliquée (fallback "{}")
                switch (log.ActionType)
                {
                    case AuditActionType.Create:
                        log.OldValues = log.OldValues ?? "{}";      // aucune ancienne valeur
                        log.NewValues = log.NewValues ?? "{}";      // valeurs créées
                        break;
                    case AuditActionType.Delete:
                        log.OldValues = log.OldValues ?? "{}";      // valeurs avant suppression
                        log.NewValues = log.NewValues ?? "{}";      // pas de nouvelles valeurs => placeholder
                        break;
                    case AuditActionType.Update:
                    case AuditActionType.Restore:
                    case AuditActionType.StatusChange:
                        log.OldValues = log.OldValues ?? "{}";
                        log.NewValues = log.NewValues ?? "{}";
                        break;
                    default:
                        log.OldValues = log.OldValues ?? "{}";
                        log.NewValues = log.NewValues ?? "{}";
                        break;
                }

                _context.AuditLogs.Add(log);
                _context.SaveChanges();
            }
        }

        public IList<AuditLog> GetAll(int count = 100)
        {
            return _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();
        }

        public IList<AuditLog> GetByUser(string userId, int count = 50)
        {
            return _context.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();
        }

        public IList<AuditLog> GetByEntityType(AuditEntityType entityType, int count = 50)
        {
            return _context.AuditLogs
                .Where(a => a.EntityType == entityType)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();
        }

        public IList<AuditLog> GetByDateRange(DateTime from, DateTime to)
        {
            return _context.AuditLogs
                .Where(a => a.Timestamp >= from && a.Timestamp <= to)
                .OrderByDescending(a => a.Timestamp)
                .ToList();
        }

        public IList<AuditLog> GetByDate(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            return GetByDateRange(startDate, endDate);
        }
    }
}
