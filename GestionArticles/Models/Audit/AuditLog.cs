namespace GestionArticles.Models.Audit
{
    /// <summary>
    /// Type d'action auditée
    /// </summary>
    public enum AuditActionType
    {
        Create = 1,
        Update = 2,
        Delete = 3,
        Restore = 4,
        StatusChange = 5
    }

    /// <summary>
    /// Type d'entité auditée
    /// </summary>
    public enum AuditEntityType
    {
        Product = 1,
        Category = 2,
        Order = 3,
        Stock = 4,
        User = 5
    }

    /// <summary>
    /// Modèle pour tracer les actions admin/manager
    /// </summary>
    public class AuditLog
    {
        public int Id { get; set; }
        
        // Qui a fait l'action (nullable si inconnu)
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        
        // Quoi a été fait
        public AuditActionType ActionType { get; set; }
        public AuditEntityType EntityType { get; set; }
        public int EntityId { get; set; }              // ID du produit, commande, etc.
        public string? EntityName { get; set; }        // Nom du produit, etc.
        
        // Changements (avant/après) – peuvent être absents selon l'action
        public string? OldValues { get; set; }         // JSON des anciennes valeurs
        public string? NewValues { get; set; }         // JSON des nouvelles valeurs
        
        // Quand
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        // IP client
        public string? IpAddress { get; set; }
    }
}
