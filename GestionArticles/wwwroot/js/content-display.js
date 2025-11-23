// CONTENT DISPLAY FORCE SCRIPT
// This script forces all content to be visible and properly displayed

document.addEventListener('DOMContentLoaded', function() {
    console.log('Content Display Script Loaded');
    
    // Force all elements to be visible
    function forceContentVisibility() {
        // Force main content
        const main = document.querySelector('main');
        if (main) {
            main.style.display = 'block';
            main.style.visibility = 'visible';
            main.style.opacity = '1';
            main.style.background = '#ffffff';
            main.style.minHeight = '60vh';
            main.style.position = 'relative';
            main.style.zIndex = '10';
        }
        
        // Force all sections
        const sections = document.querySelectorAll('section');
        sections.forEach(section => {
            section.style.display = 'block';
            section.style.visibility = 'visible';
            section.style.opacity = '1';
            section.style.background = '#ffffff';
            section.style.minHeight = '200px';
            section.style.position = 'relative';
            section.style.zIndex = '5';
        });
        
        // Force containers
        const containers = document.querySelectorAll('.container');
        containers.forEach(container => {
            container.style.display = 'block';
            container.style.visibility = 'visible';
            container.style.opacity = '1';
            container.style.position = 'relative';
            container.style.zIndex = '15';
        });
        
        // Force rows
        const rows = document.querySelectorAll('.row');
        rows.forEach(row => {
            row.style.display = 'flex';
            row.style.visibility = 'visible';
            row.style.opacity = '1';
            row.style.flexWrap = 'wrap';
        });
        
        // Force columns
        const cols = document.querySelectorAll('[class*="col-"]');
        cols.forEach(col => {
            col.style.display = 'block';
            col.style.visibility = 'visible';
            col.style.opacity = '1';
            col.style.position = 'relative';
            col.style.zIndex = '8';
        });
        
        // Force cards
        const cards = document.querySelectorAll('.card');
        cards.forEach(card => {
            card.style.display = 'block';
            card.style.visibility = 'visible';
            card.style.opacity = '1';
            card.style.background = '#ffffff';
            card.style.border = '1px solid #e5e7eb';
            card.style.boxShadow = '0 4px 6px -1px rgba(0, 0, 0, 0.1)';
            card.style.position = 'relative';
            card.style.zIndex = '6';
        });
        
        // Force text elements
        const textElements = document.querySelectorAll('h1, h2, h3, h4, h5, h6, p, span, div, a, button, label');
        textElements.forEach(element => {
            element.style.display = element.tagName === 'A' || element.tagName === 'BUTTON' || element.tagName === 'SPAN' ? 'inline-block' : 'block';
            element.style.visibility = 'visible';
            element.style.opacity = '1';
            element.style.position = 'relative';
            element.style.zIndex = '7';
        });
        
        // Force buttons
        const buttons = document.querySelectorAll('.btn');
        buttons.forEach(button => {
            button.style.display = 'inline-block';
            button.style.visibility = 'visible';
            button.style.opacity = '1';
            button.style.position = 'relative';
            button.style.zIndex = '9';
        });
        
        // Force hero section
        const heroSection = document.querySelector('.hero-section');
        if (heroSection) {
            heroSection.style.display = 'flex';
            heroSection.style.visibility = 'visible';
            heroSection.style.opacity = '1';
            heroSection.style.background = 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
            heroSection.style.color = 'white';
            heroSection.style.padding = '80px 0';
            heroSection.style.minHeight = '400px';
            heroSection.style.alignItems = 'center';
            heroSection.style.position = 'relative';
            heroSection.style.zIndex = '5';
        }
        
        // Force all elements to be visible
        const allElements = document.querySelectorAll('*');
        allElements.forEach(element => {
            if (element.style.visibility === 'hidden') {
                element.style.visibility = 'visible';
            }
            if (element.style.display === 'none') {
                element.style.display = 'block';
            }
            if (element.style.opacity === '0') {
                element.style.opacity = '1';
            }
        });
        
        console.log('Content visibility forced');
    }
    
    // Run immediately
    forceContentVisibility();
    
    // Run after a short delay to ensure all content is loaded
    setTimeout(forceContentVisibility, 100);
    setTimeout(forceContentVisibility, 500);
    setTimeout(forceContentVisibility, 1000);
    
    // Run on window load
    window.addEventListener('load', forceContentVisibility);
    
    // Run on any DOM changes
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.type === 'childList') {
                setTimeout(forceContentVisibility, 50);
            }
        });
    });
    
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
    
    console.log('Content Display Script Initialized');
});
