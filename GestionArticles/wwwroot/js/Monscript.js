// Show loading spinner
function showLoading() {
    $('#loadingSpinner').removeClass('d-none');
}

// Hide loading spinner
function hideLoading() {
    $('#loadingSpinner').addClass('d-none');
}

function parseNumber(value) {
    if (value == null) return NaN;
    if (typeof value === 'number') return value;
    // remove currency symbols and non-numeric chars except + - . ,
    var cleaned = String(value).replace(/[^0-9,.-]+/g, '').trim();
    // replace comma as decimal separator if present and there is only one comma
    if ((cleaned.match(/,/g) || []).length === 1 && cleaned.indexOf('.') === -1) {
        cleaned = cleaned.replace(',', '.');
    } else {
        // remove thousands separators (spaces, commas) if multiple
        cleaned = cleaned.replace(/[, ]+/g, '');
    }
    var n = parseFloat(cleaned);
    return isNaN(n) ? NaN : n;
}

function formatCurrency(value) {
    var num = parseNumber(value);
    if (isNaN(num)) return '0 €';
    return num.toLocaleString('fr-FR', { style: 'currency', currency: 'EUR', minimumFractionDigits: 0, maximumFractionDigits: 0 });
}

// Helper to get first available property (case-insensitive/camelCase)
function getVal(obj, keys) {
    if (!obj) return undefined;
    for (var i = 0; i < keys.length; i++) {
        var k = keys[i];
        if (obj.hasOwnProperty(k) && obj[k] !== undefined && obj[k] !== null) return obj[k];
        // try camelCase/lowercase variant
        var k2 = k.charAt(0).toLowerCase() + k.slice(1);
        if (obj.hasOwnProperty(k2) && obj[k2] !== undefined && obj[k2] !== null) return obj[k2];
        var k3 = k.charAt(0).toUpperCase() + k.slice(1);
        if (obj.hasOwnProperty(k3) && obj[k3] !== undefined && obj[k3] !== null) return obj[k3];
    }
    return undefined;
}

// Update cart total display
function updateCartTotal(total) {
    if (total == null) return;
    var num = parseNumber(total);
    if (isNaN(num)) {
        // don't overwrite with NaN, just exit
        console.warn('updateCartTotal: total is NaN, value=', total);
        return;
    }
    var formatted = num.toLocaleString('fr-FR', {
        style: 'currency',
        currency: 'EUR',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    });
    $('#totalapayer').text(formatted);
    $('#cart-total').text(formatted);
}

// Add visual feedback to buttons
function addButtonFeedback(button) {
    button.addClass('btn-loading');
    button.prop('disabled', true);
    setTimeout(() => {
        button.removeClass('btn-loading');
        button.prop('disabled', false);
    }, 1000);
}

function updateAllQuantityDisplays(productId, quantity) {
    // numeric quantity
    var q = parseInt(quantity || 0, 10);
    if (isNaN(q)) q = 0;
    // #quantite_<id>
    $("#quantite_" + productId).text(q);
    // .btn-light in .cart-item (first)
    $("#row-" + productId + " .btn-light").first().text(q);
    // data-qty attribute on row
    $("#row-" + productId).attr('data-qty', q);
}

function setRowTotal(productId, totalRow) {
    var num = parseNumber(totalRow);
    var formatted = isNaN(num) ? '0 €' : num.toLocaleString('fr-FR', { style: 'currency', currency: 'EUR', minimumFractionDigits: 0, maximumFractionDigits: 0 });
    var sel = $("#total_" + productId);
    if (sel.length) sel.text(formatted);
    else $("#row-" + productId + " #total_" + productId).text(formatted);
}

// Unified getter for quantity from response
function readQuantityFromResponse(data) {
    return getVal(data, ['quantity', 'quatite', 'Quatite', 'Quatité', 'Quatité']);
}

// Unified getter for cart total from response
function readCartTotalFromResponse(data) {
    return getVal(data, ['cartTotal', 'total', 'Total', 'carttotal']);
}

// Unified getter for row total
function readRowTotalFromResponse(data) {
    return getVal(data, ['rowTotal', 'totalRow', 'TotalRow', 'rowtotal']);
}

$(document).on("click", ".PlusProducts", function (event) {
    event.preventDefault();
    var button = $(this);
    var recordtoupdate = button.attr("data-id");
    if (!recordtoupdate) return;

    button.prop('disabled', true);
    showLoading();
    $.post("/Panier/PlusProduct", { id: recordtoupdate }, function (data) {
        console.log('PlusProduct response:', data);
        hideLoading();
        button.prop('disabled', false);

        if (data && (getVal(data,['success','ct','removed']) !== undefined)) {
            var removed = !!getVal(data, ['removed']);
            var qty = readQuantityFromResponse(data);
            var cartTotal = readCartTotalFromResponse(data);
            var rowTotal = readRowTotalFromResponse(data);

            if (removed) {
                var row = $("#row-" + recordtoupdate);
                if (row.length) row.fadeOut('fast', function () { $(this).remove(); });
                updateCartTotal(cartTotal || getVal(data,['Total', 'total']));
                showToast('info', data.message || 'Article supprimé du panier');
                if ($('[id^="row-"]').length === 0) location.reload();
                return;
            }

            // update UI
            updateCartTotal(cartTotal || getVal(data,['Total','total']));
            updateAllQuantityDisplays(recordtoupdate, qty);
            setRowTotal(recordtoupdate, rowTotal || getVal(data,['TotalRow','totalRow','totalRow']));

            var maxQ = getVal(data, ['maxQuantity', 'maxquantity']);
            var qtyNum = parseInt(qty || 0, 10);
            if (maxQ !== undefined && qtyNum >= parseInt(maxQ, 10)) {
                button.prop('disabled', true);
                button.addClass('btn-outline-secondary');
                button.removeClass('btn-outline-primary');
                button.html('<i class="fas fa-ban"></i>');
                button.attr('title', 'Stock maximum atteint');
            }

            $("#quantite_" + recordtoupdate).addClass('text-success');
            setTimeout(() => { $("#quantite_" + recordtoupdate).removeClass('text-success'); }, 1000);

            // sanity check: compare displayed with server value
            var displayed = parseInt($("#quantite_" + recordtoupdate).text() || '0', 10);
            if (displayed !== parseInt(qty || 0, 10)) {
                console.warn('Quantity mismatch after plus - reloading to sync', displayed, qty);
                location.reload();
                return;
            }

            showToast('success', data.message || 'Quantité mise à jour');
        } else {
            showToast('error', data && data.message ? data.message : 'Erreur lors de la mise à jour');
        }
    }).fail(function () {
        hideLoading();
        button.prop('disabled', false);
        showToast('error', 'Erreur de connexion lors de la mise à jour');
    });
});

$(document).on("click", ".MinProducts", function (event) {
    event.preventDefault();
    var button = $(this);
    var recordtoupdate = button.attr("data-id");
    if (!recordtoupdate) return;

    button.prop('disabled', true);
    showLoading();
    $.post("/Panier/MinusProduct", { id: recordtoupdate }, function (data) {
        console.log('MinusProduct response:', data);
        hideLoading();
        button.prop('disabled', false);

        if (data && (getVal(data,['success','ct','removed']) !== undefined)) {
            var removed = !!getVal(data, ['removed']);
            var qty = readQuantityFromResponse(data);
            var cartTotal = readCartTotalFromResponse(data);
            var rowTotal = readRowTotalFromResponse(data);

            if (removed) {
                var row = $("#row-" + recordtoupdate);
                if (row.length) row.fadeOut('fast', function () { $(this).remove(); });
                updateCartTotal(cartTotal || getVal(data,['Total','total']));
                showToast('info', data.message || 'Article supprimé du panier');
                if ($('[id^="row-"]').length === 0) location.reload();
                return;
            }

            updateCartTotal(cartTotal || getVal(data,['Total','total']));
            updateAllQuantityDisplays(recordtoupdate, qty);
            setRowTotal(recordtoupdate, rowTotal || getVal(data,['TotalRow','totalRow']));

            // enable plus if it was disabled
            var plusButton = $(".PlusProducts[data-id='" + recordtoupdate + "']");
            var maxQ = getVal(data, ['maxQuantity', 'maxquantity']);
            if (maxQ !== undefined && parseInt(qty || 0, 10) < parseInt(maxQ, 10)) {
                plusButton.prop('disabled', false);
                plusButton.removeClass('btn-outline-secondary');
                plusButton.addClass('btn-outline-primary');
                plusButton.html('+');
                plusButton.attr('title', '');
            }

            $("#quantite_" + recordtoupdate).addClass('text-warning');
            setTimeout(() => { $("#quantite_" + recordtoupdate).removeClass('text-warning'); }, 1000);

            var displayed = parseInt($("#quantite_" + recordtoupdate).text() || '0', 10);
            if (displayed !== parseInt(qty || 0, 10)) {
                console.warn('Quantity mismatch after minus - reloading to sync', displayed, qty);
                location.reload();
                return;
            }

            showToast('success', data.message || 'Quantité mise à jour');
        } else {
            showToast('error', data && data.message ? data.message : 'Erreur lors de la mise à jour');
        }
    }).fail(function () {
        hideLoading();
        button.prop('disabled', false);
        showToast('error', 'Erreur de connexion lors de la mise à jour');
    });
});

$(document).on("click", ".RemoveLink", function (event) {
    event.preventDefault();
    var button = $(this);
    var recordtoupdate = button.attr("data-id");
    if (!recordtoupdate) return;
    if (confirm('Êtes-vous sûr de vouloir supprimer cet article du panier ?')) {
        showLoading();
        addButtonFeedback(button);
        $.post("/Panier/RemoveProduct", { id: recordtoupdate }, function (data) {
            hideLoading();
            var cartTotal = readCartTotalFromResponse(data) || getVal(data,['Total','total']);
            updateCartTotal(cartTotal);
            var row = $("#row-" + recordtoupdate);
            if (row.length) {
                row.fadeOut('fast', function () { $(this).remove();
                    if ($('[id^="row-"]').length === 0) location.reload();
                });
            }
        }).fail(function () {
            hideLoading();
            alert('Erreur lors de la suppression de l\'article');
        });
    }
});

// Toast notification system
function showToast(type, message) {
    $('.toast-notification').remove();
    var toastClass = '';
    var icon = '';
    switch (type) {
        case 'success':
            toastClass = 'alert-success';
            icon = '<i class="fas fa-check-circle me-2"></i>';
            break;
        case 'error':
            toastClass = 'alert-danger';
            icon = '<i class="fas fa-exclamation-circle me-2"></i>';
            break;
        case 'warning':
            toastClass = 'alert-warning';
            icon = '<i class="fas fa-exclamation-triangle me-2"></i>';
            break;
        case 'info':
            toastClass = 'alert-info';
            icon = '<i class="fas fa-info-circle me-2"></i>';
            break;
        default:
            toastClass = 'alert-info';
            icon = '<i class="fas fa-info-circle me-2"></i>';
    }
    var toast = $(`
        <div class="toast-notification alert ${toastClass} alert-dismissible fade show position-fixed" 
             style="top: 20px; right: 20px; z-index: 9999; min-width: 300px; box-shadow: 0 4px 12px rgba(0,0,0,0.15);">
            ${icon}${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);
    $('body').append(toast);
    setTimeout(() => {
        toast.alert('close');
    }, 5000);
}

$('<style>').prop('type', 'text/css').html(`
        .btn-loading { position: relative; color: transparent !important; }
        .btn-loading:after { content: ""; position: absolute; width: 16px; height: 16px; top: 50%; left: 50%; margin-left: -8px; margin-top: -8px; border: 2px solid #ffffff; border-radius: 50%; border-top-color: transparent; animation: spin 1s linear infinite; }
        @keyframes spin { to { transform: rotate(360deg); } }
        .toast-notification { animation: slideInRight 0.3s ease-out; }
        @keyframes slideInRight { from { transform: translateX(100%); opacity: 0; } to { transform: translateX(0); opacity: 1; } }
        .btn:disabled { opacity: 0.6; cursor: not-allowed; }
        .quantity-controls { display: flex; align-items: center; gap: 0.5rem; }
        .quantity-display { min-width: 40px; text-align: center; font-weight: bold; padding: 0.25rem 0.5rem; background-color: #f8f9fa; border-radius: 0.25rem; }
    `).appendTo('head');


// Favorite toggle handler
$(document).on("click", ".FavoriteToggle", function (event) {
    event.preventDefault();
    var button = $(this);
    var productId = button.attr('data-id');
    if (!productId) return;
    button.prop('disabled', true);
    $.post('/Favorites/Toggle', { id: productId }, function (data) {
        button.prop('disabled', false);
        if (data && data.success) {
            var fav = data.favorited === true || data.favorited === 'true';
            var icon = button.find('i');
            if (fav) {
                icon.removeClass('far').addClass('fas text-danger');
                showToast('success', 'Ajouté aux favoris');
            } else {
                icon.removeClass('fas text-danger').addClass('far');
                showToast('info', 'Retiré des favoris');
                // if on favorites page, remove row
                var row = button.closest('.cart-item, .card');
                if (row.length && window.location.pathname.toLowerCase().includes('/favorites')) {
                    row.fadeOut('fast', function () { $(this).remove(); });
                }
            }
        } else {
            showToast('error', data && data.message ? data.message : 'Erreur lors de l\'opération');
        }
    }).fail(function () {
        button.prop('disabled', false);
        showToast('error', 'Erreur de connexion');
    });
});

// End of file