namespace GestionArticles.Models.Repositories
{
    using GestionArticles.Models.Notifications;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    public interface INotificationRepository
    {
        void Add(Notification notification);
        IList<Notification> GetByUserId(string userId, int count = 20);
        IList<Notification> GetUnreadByUserId(string userId);
        IList<Notification> GetAdminNotifications(int count = 20);
        IList<Notification> GetUnreadAdminNotifications();
        IList<Notification> GetAll();              // ? AJOUT
        Notification GetById(int id);
        void MarkAsRead(int notificationId);
        void MarkAllAsRead(string userId);
        void Delete(int notificationId);
        void Update(Notification notification);    // ? AJOUT
        int GetUnreadCount(string userId);
        int GetUnreadAdminCount();
    }

    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Notification notification)
        {
            notification.CreatedAt = DateTime.Now;
            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }

        public IList<Notification> GetByUserId(string userId, int count = 20)
        {
            return _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsAdmin)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToList();
        }

        public IList<Notification> GetUnreadByUserId(string userId)
        {
            return _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsAdmin && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        public IList<Notification> GetAdminNotifications(int count = 20)
        {
            return _context.Notifications
                .AsNoTracking()
                .Where(n => n.IsAdmin)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToList();
        }

        public IList<Notification> GetUnreadAdminNotifications()
        {
            return _context.Notifications
                .AsNoTracking()
                .Where(n => n.IsAdmin && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        public IList<Notification> GetAll()
        {
            return _context.Notifications
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        public Notification GetById(int id)
        {
            return _context.Notifications.Find(id);
        }

        public void MarkAsRead(int notificationId)
        {
            var notification = _context.Notifications.Find(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public void MarkAllAsRead(string userId)
        {
            var unread = _context.Notifications
                .Where(n => n.UserId == userId && !n.IsAdmin && !n.IsRead)
                .ToList();

            foreach (var notification in unread)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
            }

            _context.SaveChanges();
        }

        public void Delete(int notificationId)
        {
            var notification = _context.Notifications.Find(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                _context.SaveChanges();
            }
        }

        public void Update(Notification notification)
        {
            _context.Notifications.Update(notification);
            _context.SaveChanges();
        }

        public int GetUnreadCount(string userId)
        {
            return _context.Notifications
                .Count(n => n.UserId == userId && !n.IsAdmin && !n.IsRead);
        }

        public int GetUnreadAdminCount()
        {
            return _context.Notifications
                .Count(n => n.IsAdmin && !n.IsRead);
        }
    }
}
