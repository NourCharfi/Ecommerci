/**
 * Global Currency Formatter: remplace € par DT
 * Format: 10 000 DT (fr-FR)
 * Supprime ,00 pour les nombres entiers
 */

window.CurrencyFormatter = {
    format: function(value, decimals = 0) {
        if (typeof value !== 'number') value = parseFloat(value) || 0;
        let formatted = new Intl.NumberFormat('fr-FR', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        }).format(value);
        if (decimals === 0 && value % 1 === 0) {
            formatted = formatted.replace(/,00$/, '');
        }
        return formatted + ' DT';
    },
    formatNoSuffix: function(value, decimals = 0) {
        if (typeof value !== 'number') value = parseFloat(value) || 0;
        let formatted = new Intl.NumberFormat('fr-FR', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        }).format(value);
        if (decimals === 0 && value % 1 === 0) {
            formatted = formatted.replace(/,00$/, '');
        }
        return formatted;
    },
    replaceCurrency: function() {
        document.querySelectorAll('body *').forEach(el => {
            if (el.children.length === 0) {
                let text = el.textContent;
                text = text.replace(/€/g, 'DT')
                           .replace(/EUR/g, 'DT')
                           .replace(/\beur\b/gi, 'dt');
                text = text.replace(/(\d+),00(\s+DT)/g, '$1$2');
                text = text.replace(/(\d+),00\b/g, '$1');
                el.textContent = text;
            }
        });
    }
};

document.addEventListener('DOMContentLoaded', function() {
    CurrencyFormatter.replaceCurrency();
});
