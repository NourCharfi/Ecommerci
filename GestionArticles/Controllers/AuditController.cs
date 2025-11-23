using GestionArticles.Models.Repositories;
using GestionArticles.Models.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionArticles.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditController : Controller
    {
        private readonly IAuditLogRepository _auditRepository;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IAuditLogRepository auditRepository, ILogger<AuditController> logger)
        {
            _auditRepository = auditRepository;
            _logger = logger;
        }

        /// <summary>
        /// Afficher l'historique complet
        /// </summary>
        public IActionResult Index(string period = "all")
        {
            IList<AuditLog> logs;

            switch (period)
            {
                case "today":
                    logs = _auditRepository.GetByDate(DateTime.Today);
                    ViewBag.Period = "Aujourd'hui";
                    break;
                case "week":
                    var weekAgo = DateTime.Today.AddDays(-7);
                    logs = _auditRepository.GetByDateRange(weekAgo, DateTime.Now);
                    ViewBag.Period = "Cette semaine";
                    break;
                case "month":
                    var monthAgo = DateTime.Today.AddDays(-30);
                    logs = _auditRepository.GetByDateRange(monthAgo, DateTime.Now);
                    ViewBag.Period = "Ce mois";
                    break;
                case "year":
                    var yearAgo = DateTime.Today.AddDays(-365);
                    logs = _auditRepository.GetByDateRange(yearAgo, DateTime.Now);
                    ViewBag.Period = "Cette année";
                    break;
                default:
                    logs = _auditRepository.GetAll(1000);
                    ViewBag.Period = "Tout l'historique";
                    break;
            }

            ViewBag.SelectedPeriod = period;
            return View(logs);
        }

        /// <summary>
        /// Filtrer par type d'entité
        /// </summary>
        public IActionResult ByEntity(AuditEntityType entityType)
        {
            var logs = _auditRepository.GetByEntityType(entityType, 500);
            ViewBag.EntityType = entityType;
            return View("Index", logs);
        }

        /// <summary>
        /// Détails d'un log d'audit
        /// </summary>
        [HttpGet]
        public IActionResult Details(int id)
        {
            var log = _auditRepository.GetAll(5000).FirstOrDefault(l => l.Id == id);
            if (log == null)
                return NotFound();
            return View(log);
        }
    }
}
