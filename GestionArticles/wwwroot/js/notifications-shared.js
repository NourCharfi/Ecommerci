// ========================================
// ?? NOTIFICATIONS SHARED FUNCTIONS
// ========================================

let allNotificationsShared = [];
let notificationLoadTimeout = null;

/**
 * Charge les notifications depuis l'API
 */
function loadNotificationsShared() {
    clearTimeout(notificationLoadTimeout);
    
    fetch('/api/notifications/list?count=100')
        .then(response => {
            if (!response.ok) {
                console.warn('? API status:', response.status);
                throw new Error('HTTP ' + response.status);
            }
            return response.json();
        })
        .then(notifications => {
            console.log('? Notifications chargées (shared):', notifications);
            
            // Valider que c'est un array
            if (!Array.isArray(notifications)) {
                console.warn('?? Response not array:', notifications);
                notifications = [];
            }
            
            allNotificationsShared = notifications;
            
            // Mettre à jour le dropdown SI cloche existe
            const notificationBell = document.getElementById('notificationBell');
            if (notificationBell) {
                console.log('?? Updating dropdown');
                updateDropdownNotifications(notifications);
            }
            
            // Mettre à jour la page /Notifications SI la page de notifications existe
            const pageCountAll = document.getElementById('countAll');
            if (pageCountAll) {
                console.log('?? Updating notifications page');
                updatePageNotifications(notifications);
            }
            
            // Reschedule dans 30s
            notificationLoadTimeout = setTimeout(loadNotificationsShared, 30000);
        })
        .catch(err => {
            console.error('? Erreur notifications:', err.message);
            
            // Afficher erreur dans dropdown
            const dropdownList = document.querySelector('#notificationBell + .dropdown-menu #notificationsList');
            if (dropdownList) {
                dropdownList.innerHTML = '<div class="text-center text-muted py-4"><i class="fas fa-exclamation-circle fs-4"></i><small>Erreur</small></div>';
            }
            
            // Retry dans 5s
            notificationLoadTimeout = setTimeout(loadNotificationsShared, 5000);
        });
}

/**
 * Met à jour le dropdown de la cloche
 */
function updateDropdownNotifications(notifications) {
    if (!Array.isArray(notifications)) return;
    
    // Mettre à jour le badge
    const badge = document.getElementById('notificationBadge');
    const countSpan = badge ? (badge.querySelector('#unreadCount') || badge.querySelector('span')) : null;
    const unread = notifications.filter(n => n && !n.isRead).length;
    
    if (badge && countSpan) {
        if (unread > 0) {
            countSpan.textContent = unread;
            badge.style.display = 'inline-block';
        } else {
            badge.style.display = 'none';
        }
    }
    
    // Mettre à jour la liste du dropdown
    const dropdownList = document.querySelector('#notificationBell + .dropdown-menu #notificationsList');
    if (!dropdownList) return;
    
    if (notifications.length === 0) {
        dropdownList.innerHTML = '<div class="text-center text-muted py-4"><i class="fas fa-inbox fs-4 mb-2"></i><small>Aucune notification</small></div>';
    } else {
        dropdownList.innerHTML = notifications.map(n => {
            if (!n || !n.id) return '';
            return `<div class="notification-item ${!n.isRead ? 'unread' : ''}" data-id="${n.id}" data-action="${n.actionUrl || '#'}" role="button" tabindex="0">
                <div class="notification-icon ${n.color || 'info'}">
                    <i class="fas ${n.icon || 'fa-bell'}"></i>
                </div>
                <div class="notification-content">
                    <p class="notification-title">${n.title || 'Notification'}</p>
                    <p class="notification-message">${n.message || ''}</p>
                    <div class="notification-time">${formatTimeRelative(new Date(n.createdAt))}</div>
                </div>
                <button class="btn btn-sm btn-link text-danger notification-close" onclick="event.stopPropagation(); deleteNotification(${n.id}); return false;" title="Supprimer">
                    <i class="fas fa-times"></i>
                </button>
            </div>`;
        }).join('');
    }
}

/**
 * Met à jour la page /Notifications
 */
function updatePageNotifications(notifications) {
    if (!Array.isArray(notifications)) return;
    
    const countAllEl = document.getElementById('countAll');
    const countUnreadEl = document.getElementById('countUnread');
    
    if (countAllEl) {
        countAllEl.textContent = notifications.length;
        console.log('? Updated countAll:', notifications.length);
    }
    
    if (countUnreadEl) {
        countUnreadEl.textContent = notifications.filter(n => !n.isRead).length;
        console.log('? Updated countUnread:', notifications.filter(n => !n.isRead).length);
    }
    
    // Afficher les notifications
    displayPageNotifications(notifications);
}

/**
 * Affiche les notifications sur la page /Notifications
 */
function displayPageNotifications(notifications) {
    const container = document.getElementById('notificationsPageList');
    if (!container) return;
    
    // Vérifier que c'est bien la page de notifications (elle a countAll)
    if (!document.getElementById('countAll')) {
        console.log('?? Not on notifications page');
        return;
    }
    
    console.log('?? Displaying page notifications:', notifications.length);
    
    if (!notifications || notifications.length === 0) {
        container.innerHTML = `<div class="text-center text-muted py-5">
            <i class="fas fa-inbox fs-1 mb-3 d-block opacity-50"></i>
            <p>Aucune notification pour le moment</p>
        </div>`;
        return;
    }
    
    const html = notifications.map(n => {
        if (!n || !n.id) return '';
        return `<div class="card notification-card ${n.color || 'info'} ${!n.isRead ? 'unread' : ''} mb-3" 
                 onclick="window.location.href='${n.actionUrl || '#'}'" 
                 style="cursor: pointer;">
            <div class="card-body">
                <div class="row">
                    <div class="col-auto">
                        <div class="notification-badge ${n.color || 'info'}">
                            <i class="fas ${n.icon || 'fa-bell'}"></i>
                        </div>
                    </div>
                    <div class="col">
                        <div class="d-flex justify-content-between align-items-start">
                            <div>
                                <h5 class="card-title mb-1">${n.title || 'Notification'}</h5>
                                <p class="card-text text-muted mb-1">${n.message || ''}</p>
                                <small class="notification-time">${formatTimeRelative(new Date(n.createdAt))}</small>
                            </div>
                            <div>
                                ${!n.isRead ? '<span class="mark-read-badge">NOUVEAU</span>' : ''}
                            </div>
                        </div>
                    </div>
                    <div class="col-auto">
                        <button class="btn btn-sm btn-link text-danger" 
                                onclick="event.stopPropagation(); deleteNotification(${n.id})">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>`;
    }).join('');
    
    container.innerHTML = html || `<div class="text-center text-muted py-5">
        <i class="fas fa-inbox fs-1 mb-3 d-block opacity-50"></i>
        <p>Aucune notification pour le moment</p>
    </div>`;
}

/**
 * Formate le temps de manière relative
 */
function formatTimeRelative(date) {
    if (!(date instanceof Date)) return '?';
    const now = new Date(), diff = now - date;
    const m = Math.floor(diff / 60000), h = Math.floor(diff / 3600000), d = Math.floor(diff / 86400000);
    if (m < 1) return 'à l\'instant';
    if (m < 60) return m + 'm';
    if (h < 24) return h + 'h';
    if (d < 7) return d + 'j';
    return date.toLocaleDateString('fr-FR');
}

/**
 * Supprime une notification
 */
function deleteNotification(id) {
    fetch(`/api/notifications/${id}`, { method: 'DELETE' })
        .catch(err => console.error('? Delete error:', err))
        .finally(() => loadNotificationsShared());
}

/**
 * Filtre les notifications
 */
function filterNotificationsShared(filter) {
    let filtered = allNotificationsShared;

    if (filter === 'unread') {
        filtered = filtered.filter(n => !n.isRead);
    } else if (filter === 'order') {
        filtered = filtered.filter(n => 
            ['OrderCreated', 'OrderConfirmed', 'OrderShipping', 'OrderDelivered'].includes(n.type)
        );
    } else if (filter === 'payment') {
        filtered = filtered.filter(n => 
            ['PaymentSuccessful', 'PaymentFailed'].includes(n.type)
        );
    } else if (filter === 'product') {
        filtered = filtered.filter(n => 
            ['NewProductAdded', 'ProductRestock'].includes(n.type)
        );
    }

    displayPageNotifications(filtered);
}

// Auto-load quand la page se charge
document.addEventListener('DOMContentLoaded', function() {
    console.log('?? Loading notifications...');
    loadNotificationsShared();
});
