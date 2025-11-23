// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// ============================================
// MODERN E-COMMERCE - SITE INTERACTIONS
// ============================================

document.addEventListener("DOMContentLoaded", function () {
    // ============================================
    // Sidebar Toggle
    // ============================================
    var sidebar = document.getElementById("sidebar");
    var mainContent = document.getElementById("mainContent");
    var toggleButton = document.getElementById("toggleMenu");

    if (sidebar && mainContent && toggleButton) {
        toggleButton.addEventListener("click", function () {
            if (sidebar.style.display === "none") {
                sidebar.style.display = "block";
                mainContent.classList.remove("col-md-12");
                mainContent.classList.add("col-md-10");
            } else {
                sidebar.style.display = "none";
                mainContent.classList.remove("col-md-10");
                mainContent.classList.add("col-md-12");
            }
        });
    }

    // ============================================
    // Smooth Scroll for Anchor Links
    // ============================================
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href !== '#' && href.length > 1) {
                e.preventDefault();
                const target = document.querySelector(href);
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            }
        });
    });

    // ============================================
    // Active Navigation Highlighting
    // ============================================
    const currentPath = window.location.pathname;
    document.querySelectorAll('.navbar-nav .nav-link').forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        }
    });

    // ============================================
    // Image Lazy Loading
    // ============================================
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                if (img.dataset.src) {
                    img.src = img.dataset.src;
                    img.removeAttribute('data-src');
                    observer.unobserve(img);
                }
            }
        });
    });

    document.querySelectorAll('img[data-src]').forEach(img => {
        imageObserver.observe(img);
    });

    // ============================================
    // Animate Elements on Scroll
    // ============================================
    const animateObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
                animateObserver.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    });

    document.querySelectorAll('.product-card, .category-card, .dashboard-card').forEach(el => {
        animateObserver.observe(el);
    });

    // ============================================
    // Update Cart Badge Count
    // ============================================
    function updateCartBadge() {
        // Cette fonction sera appelée après l'ajout d'articles au panier
        // Pour l'instant, on peut la laisser vide ou récupérer le compte du serveur
        const cartBadge = document.querySelector('.cart-badge');
        if (cartBadge) {
            // Vous pouvez faire un appel AJAX pour obtenir le nombre d'articles
            // ou utiliser le localStorage
            const cartCount = localStorage.getItem('cartCount') || '0';
            cartBadge.textContent = cartCount;
            if (cartCount === '0') {
                cartBadge.style.display = 'none';
            } else {
                cartBadge.style.display = 'block';
            }
        }
    }
    
    updateCartBadge();

    // ============================================
    // Toast Notifications
    // ============================================
    window.showToast = function(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} position-fixed`;
        toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px; animation: slideIn 0.3s ease-out;';
        toast.innerHTML = `
            <div class="d-flex align-items-center justify-content-between">
                <span>${message}</span>
                <button type="button" class="btn-close ms-3" onclick="this.parentElement.parentElement.remove()"></button>
            </div>
        `;
        
        document.body.appendChild(toast);
        
        setTimeout(() => {
            toast.style.animation = 'slideOut 0.3s ease-out';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    };

    // ============================================
    // Form Validation Enhancement
    // ============================================
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', function(e) {
            const requiredInputs = form.querySelectorAll('[required]');
            let isValid = true;

            requiredInputs.forEach(input => {
                if (!input.value.trim()) {
                    isValid = false;
                    input.classList.add('is-invalid');
                } else {
                    input.classList.remove('is-invalid');
                }
            });

            if (!isValid) {
                e.preventDefault();
                showToast('Veuillez remplir tous les champs obligatoires', 'warning');
            }
        });

        // Remove invalid class on input
        form.querySelectorAll('input, select, textarea').forEach(input => {
            input.addEventListener('input', function() {
                this.classList.remove('is-invalid');
            });
        });
    });

    // ============================================
    // Quantity Controls Enhancement
    // ============================================
    document.querySelectorAll('.quantity-controls').forEach(control => {
        const decreaseBtn = control.querySelector('[data-action="decrease"]');
        const increaseBtn = control.querySelector('[data-action="increase"]');
        const display = control.querySelector('.quantity-display');

        if (decreaseBtn && increaseBtn && display) {
            decreaseBtn.addEventListener('click', function() {
                let value = parseInt(display.textContent);
                if (value > 1) {
                    display.textContent = value - 1;
                }
            });

            increaseBtn.addEventListener('click', function() {
                let value = parseInt(display.textContent);
                const max = parseInt(this.dataset.max) || 999;
                if (value < max) {
                    display.textContent = value + 1;
                }
            });
        }
    });

    // ============================================
    // Search Enhancement
    // ============================================
    const searchInput = document.querySelector('input[type="search"]');
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            const searchTerm = this.value.toLowerCase();
            if (searchTerm.length > 0) {
                this.style.borderColor = 'var(--primary-color)';
            } else {
                this.style.borderColor = '';
            }
        });
    }

    // ============================================
    // Back to Top Button
    // ============================================
    const backToTopBtn = document.createElement('button');
    backToTopBtn.innerHTML = '<i class="fas fa-arrow-up"></i>';
    backToTopBtn.className = 'btn btn-primary btn-icon position-fixed';
    backToTopBtn.style.cssText = 'bottom: 30px; right: 30px; z-index: 999; display: none; box-shadow: var(--shadow-lg);';
    backToTopBtn.setAttribute('title', 'Retour en haut');
    document.body.appendChild(backToTopBtn);

    window.addEventListener('scroll', function() {
        if (window.pageYOffset > 300) {
            backToTopBtn.style.display = 'flex';
        } else {
            backToTopBtn.style.display = 'none';
        }
    });

    backToTopBtn.addEventListener('click', function() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });

    // ============================================
    // Navbar Scroll Effect
    // ============================================
    let lastScrollTop = 0;
    const navbar = document.querySelector('.navbar');
    
    if (navbar) {
        window.addEventListener('scroll', function() {
            const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
            
            if (scrollTop > 100) {
                navbar.style.boxShadow = 'var(--shadow-lg)';
            } else {
                navbar.style.boxShadow = 'var(--shadow-md)';
            }
            
            lastScrollTop = scrollTop;
        });
    }

    // ============================================
    // Tooltips Initialization (if using Bootstrap)
    // ============================================
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // ============================================
    // Confirm Delete Actions
    // ============================================
    document.querySelectorAll('[data-confirm]').forEach(element => {
        element.addEventListener('click', function(e) {
            const message = this.dataset.confirm || 'Êtes-vous sûr de vouloir effectuer cette action ?';
            if (!confirm(message)) {
                e.preventDefault();
            }
        });
    });

    // ============================================
    // Auto-hide Alerts
    // ============================================
    document.querySelectorAll('.alert:not(.alert-permanent)').forEach(alert => {
        setTimeout(() => {
            alert.style.animation = 'fadeOut 0.5s ease-out';
            setTimeout(() => alert.remove(), 500);
        }, 5000);
    });
});

// ============================================
// Add fadeOut animation
// ============================================
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOut {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
    
    @keyframes fadeOut {
        from {
            opacity: 1;
        }
        to {
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);