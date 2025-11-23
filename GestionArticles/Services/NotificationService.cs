namespace GestionArticles.Services
{
    using GestionArticles.Models;
    using GestionArticles.Models.Notifications;
    using GestionArticles.Models.Repositories;
    using System;

    /// <summary>
    /// Service pour gérer les notifications
    /// </summary>
    public interface INotificationService
    {
        void NotifyOrderCreated(int orderId, string userId);
        void NotifyOrderConfirmed(int orderId, string userId);
        void NotifyOrderShipping(int orderId, string userId);
        void NotifyOrderDelivered(int orderId, string userId);
        void NotifyOrderPaid(int orderId, string userId); // ajouté
        void NotifyPaymentSuccessful(int orderId, string userId);
        void NotifyPaymentFailed(int orderId, string userId);
        void NotifyNewProduct(int productId);
        void NotifyProductRestock(int productId);
        void NotifyAdminStockLow(int productId, int quantity);
        void NotifyAdminStockRupture(int productId);
        void NotifyAdminNewUser(string userId, string userEmail);
        void NotifyAdminHighValueOrder(int orderId, decimal amount);
        void NotifyAdminPaymentReceived(int orderId, decimal amount);
    }

    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(INotificationRepository notificationRepository, ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        // ========== NOTIFICATIONS CLIENT ==========

        public void NotifyOrderCreated(int orderId, string userId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = NotificationType.OrderCreated,
                Title = "Commande Créée",
                Message = $"Votre commande #{orderId} a été créée avec succès",
                Icon = "fa-shopping-cart",
                Color = "primary",
                OrderId = orderId,
                ActionUrl = $"/Orders/Details/{orderId}",
                IsRead = false,
                IsAdmin = false
            };
            _notificationRepository.Add(notification);
            _logger.LogInformation($"Notification OrderCreated sent to user {userId}");
        }

        public void NotifyOrderConfirmed(int orderId, string userId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = NotificationType.OrderConfirmed,
                Title = "Commande Confirmée",
                Message = $"Votre commande #{orderId} a été confirmée par notre équipe.",
                Icon = "fa-check-circle",
                Color = "success",
                OrderId = orderId,
                ActionUrl = $"/Orders/Details/{orderId}",
                IsRead = false,
                IsAdmin = false
            };
            _notificationRepository.Add(notification);
            _logger.LogInformation($"Notification OrderConfirmed sent to user {userId}");
        }

        public void NotifyOrderShipping(int orderId, string userId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = NotificationType.OrderShipping,
                Title = "Commande Expédiée",
                Message = $"Votre commande #{orderId} est en route vers vous",
                Icon = "fa-truck",
                Color = "info",
                OrderId = orderId,
                ActionUrl = $"/Orders/Details/{orderId}",
                IsRead = false,
                IsAdmin = false
            };
            _notificationRepository.Add(notification);
            _logger.LogInformation($"Notification OrderShipping sent to user {userId}");
        }

        public void NotifyOrderDelivered(int orderId, string userId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = NotificationType.OrderDelivered,
                Title = "Commande Livrée",
                Message = $"Votre commande #{orderId} a été livrée avec succès",
                Icon = "fa-box-open",
                Color = "success",
                OrderId = orderId,
                ActionUrl = $"/Orders/Details/{orderId}",
                IsRead = false,
                IsAdmin = false
            };
            _notificationRepository.Add(notification);
            _logger.LogInformation($"Notification OrderDelivered sent to user {userId}");
        }

        public void NotifyOrderPaid(int orderId, string userId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = NotificationType.PaymentSuccessful,
                Title = "Paiement reçu",
                Message = $"Votre paiement pour la commande #{orderId} est enregistré. Préparation en cours.",
                Icon = "fa-credit-card",
                Color = "primary",
                OrderId = orderId,
                ActionUrl = $"/Orders/Details/{orderId}",
                IsRead = false,
                IsAdmin = false
            };
            _notificationRepository.Add(notification);
            _logger.LogInformation($"Notification OrderPaid sent to user {userId}");
        }

        public void NotifyPaymentSuccessful(int orderId, string userId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = NotificationType.PaymentSuccessful,
                Title = "Paiement Réussi",
                Message = $"Votre paiement pour la commande #{orderId} a été traité avec succès",
                Icon = "fa-credit-card",
                Color = "success",
                OrderId = orderId,
                ActionUrl = $"/Orders/Details/{orderId}",
                IsRead = false,
                IsAdmin = false
            };
            _notificationRepository.Add(notification);
            _logger.LogInformation($"Notification PaymentSuccessful sent to user {userId}");
        }

        public void NotifyPaymentFailed(int orderId, string userId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = NotificationType.PaymentFailed,
                Title = "Paiement Échoué",
                Message = $"Le paiement pour la commande #{orderId} a échoué. Veuillez réessayer",
                Icon = "fa-exclamation-triangle",
                Color = "danger",
                OrderId = orderId,
                ActionUrl = $"/Orders/Details/{orderId}",
                IsRead = false,
                IsAdmin = false
            };
            _notificationRepository.Add(notification);
            _logger.LogInformation($"Notification PaymentFailed sent to user {userId}");
        }

        public void NotifyNewProduct(int productId)
        {
            // Envoyer à tous les utilisateurs
            // À implémenter avec système de broadcast
            _logger.LogInformation($"New product notification for product {productId}");
        }

        public void NotifyProductRestock(int productId)
        {
            // Envoyer aux utilisateurs qui ont suivi ce produit
            // À implémenter avec système de wishlist
            _logger.LogInformation($"Product restock notification for product {productId}");
        }

        // ========== NOTIFICATIONS ADMIN ==========

        public void NotifyAdminStockLow(int productId, int quantity)
        {
            var notification = new Notification
            {
                UserId = null,
                Type = NotificationType.StockLow,
                Title = "Stock Faible",
                Message = $"Le produit ID#{productId} a un stock faible: {quantity} unités restantes",
                Icon = "fa-exclamation-circle",
                Color = "warning",
                ProductId = productId,
                ActionUrl = $"/Admin/Stock",
                IsRead = false,
                IsAdmin = true
            };
            _notificationRepository.Add(notification);
            _logger.LogWarning($"Admin notification StockLow for product {productId}");
        }

        public void NotifyAdminStockRupture(int productId)
        {
            var notification = new Notification
            {
                UserId = null,
                Type = NotificationType.StockRupture,
                Title = "Rupture de Stock",
                Message = $"Le produit ID#{productId} est en rupture de stock!",
                Icon = "fa-alert-circle",
                Color = "danger",
                ProductId = productId,
                ActionUrl = $"/Admin/Stock",
                IsRead = false,
                IsAdmin = true
            };
            _notificationRepository.Add(notification);
            _logger.LogError($"Admin notification StockRupture for product {productId}");
        }

        public void NotifyAdminNewUser(string userId, string userEmail)
        {
            var notification = new Notification
            {
                UserId = null,
                Type = NotificationType.NewUser,
                Title = "Nouvel Utilisateur",
                Message = $"Un nouvel utilisateur s'est inscrit: {userEmail}",
                Icon = "fa-user-plus",
                Color = "info",
                ActionUrl = $"/Admin/Users",
                IsRead = false,
                IsAdmin = true
            };
            _notificationRepository.Add(notification);
            _logger.LogInformation($"Admin notification NewUser for {userEmail}");
        }

        public void NotifyAdminHighValueOrder(int orderId, decimal amount)
        {
            var notification = new Notification
            {
                UserId = null,
                Type = NotificationType.HighValueOrder,
                Title = "Commande de Valeur Élevée",
                Message = $"Une commande de valeur élevée a été reçue (#{orderId}): {amount:C}",
                Icon = "fa-star",
                Color = "primary",
                OrderId = orderId,
                ActionUrl = $"/Orders/Details/{orderId}",
                IsRead = false,
                IsAdmin = true
            };
            _notificationRepository.Add(notification);
            _logger.LogInformation($"Admin notification HighValueOrder for order {orderId}");
        }

        public void NotifyAdminPaymentReceived(int orderId, decimal amount)
        {
            var notification = new Notification
            {
                UserId = null,
                Type = NotificationType.PaymentReceived,
                Title = "Paiement Reçu",
                Message = $"Un paiement a été reçu pour la commande #{orderId}: {amount:C}",
                Icon = "fa-money-bill-wave",
                Color = "success",
                OrderId = orderId,
                ActionUrl = $"/Orders/Details/{orderId}",
                IsRead = false,
                IsAdmin = true
            };
            _notificationRepository.Add(notification);
            _logger.LogInformation($"Admin notification PaymentReceived for order {orderId} - Amount: {amount}");
        }
    }
}
