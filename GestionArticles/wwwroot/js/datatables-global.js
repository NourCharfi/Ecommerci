/**
 * Global DataTables Configuration
 * Initialise automatiquement tous les tableaux avec classe 'datatable'
 * 
 * ?? IMPORTANT: Ce script est le SEUL responsable de l'initialisation des DataTables
 * Ne pas utiliser datatables-config.js en même temps!
 */

// Configuration par défaut
const dataTablesDefaults = {
    language: {
        url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/fr-FR.json'
    },
    dom: 'Bfrtip',
    buttons: [
        {
            extend: 'copy',
            text: '<i class="fas fa-copy me-1"></i>Copier',
            className: 'btn btn-sm btn-secondary'
        },
        {
            extend: 'csv',
            text: '<i class="fas fa-download me-1"></i>CSV',
            className: 'btn btn-sm btn-secondary'
        },
        {
            extend: 'excel',
            text: '<i class="fas fa-file-excel me-1"></i>Excel',
            className: 'btn btn-sm btn-success'
        },
        {
            extend: 'pdf',
            text: '<i class="fas fa-file-pdf me-1"></i>PDF',
            className: 'btn btn-sm btn-danger'
        },
        {
            extend: 'print',
            text: '<i class="fas fa-print me-1"></i>Imprimer',
            className: 'btn btn-sm btn-info'
        }
    ],
    responsive: true,
    pageLength: 25,
    lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
    processing: true,
    serverSide: false,
    columnDefs: [
        {
            targets: -1, // Dernière colonne (actions)
            orderable: false,
            searchable: false
        }
    ],
    order: [[0, 'desc']], // Tri par défaut: première colonne descendante
    searching: true,
    paging: true,
    info: true
};

// Flag global pour éviter les double-initialisations
window.dataTablesInitialized = false;

// Initialiser DataTables au chargement DOM
document.addEventListener('DOMContentLoaded', function() {
    // Éviter les multiples initialisations
    if (window.dataTablesInitialized) {
        console.warn('?? DataTables already initialized. Skipping duplicate initialization.');
        return;
    }

    // Chercher tous les tableaux avec classe 'datatable'
    const tables = document.querySelectorAll('table.datatable');
    
    if (tables.length === 0) {
        return;
    }

    tables.forEach((table, index) => {
        try {
            // Vérifier si DataTables est déjà initialisé sur ce tableau
            if ($.fn.DataTable.isDataTable(table)) {
                console.warn(`?? Table ${table.id || 'table-' + index} already initialized. Skipping.`);
                return;
            }

            // Vérifier si DataTables est disponible
            if (typeof $ !== 'undefined' && $.fn.dataTable) {
                const config = Object.assign({}, dataTablesDefaults);
                
                // Adapter les options selon les data-attributes du tableau
                if (table.hasAttribute('data-no-buttons')) {
                    config.dom = 'frtip';
                    config.buttons = [];
                }
                if (table.hasAttribute('data-no-search')) {
                    config.searching = false;
                }
                if (table.hasAttribute('data-no-paging')) {
                    config.paging = false;
                }
                
                const pageLen = table.getAttribute('data-page-length');
                if (pageLen) config.pageLength = parseInt(pageLen);

                $(table).DataTable(config);
                console.log(`? DataTable initialized: ${table.id || 'table-' + index}`);
            }
        } catch (err) {
            console.error('? DataTable init error:', err);
        }
    });

    window.dataTablesInitialized = true;
    console.log(`? All DataTables initialized successfully (${tables.length} tables)`);
});
