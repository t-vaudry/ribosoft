// Import modern CSS and styles
import './css/site.scss';
import 'jquery-ui-themes/themes/redmond/jquery-ui.css';
import 'jquery-ui-themes/themes/redmond/theme.css';
import 'structured-filter/css/structured-filter.css';

// Bootstrap initialization function
function initializeBootstrap() {
    if (!window.bootstrap) {
        console.error('Bootstrap not found in vendor bundle');
        return;
    }

    console.log('Bootstrap found, version:', window.bootstrap.Tooltip?.VERSION || 'unknown');

    try {
        // Initialize all Bootstrap 5 dropdowns
        const dropdownElementList = [].slice.call(document.querySelectorAll('[data-bs-toggle="dropdown"]'));
        const dropdownList = dropdownElementList.map(function (dropdownToggleEl) {
            return new window.bootstrap.Dropdown(dropdownToggleEl);
        });

        // Initialize tooltips
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new window.bootstrap.Tooltip(tooltipTriggerEl);
        });

        // Initialize popovers
        const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
        const popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
            return new window.bootstrap.Popover(popoverTriggerEl);
        });

        console.log('Bootstrap components initialized successfully:', {
            dropdowns: dropdownList.length,
            tooltips: tooltipList.length,
            popovers: popoverList.length
        });

        // Test Bootstrap functionality
        console.log('Bootstrap classes available:', {
            Dropdown: !!window.bootstrap.Dropdown,
            Modal: !!window.bootstrap.Modal,
            Tooltip: !!window.bootstrap.Tooltip,
            Popover: !!window.bootstrap.Popover
        });

    } catch (error) {
        console.error('Error initializing Bootstrap components:', error);
    }
}

// Multiple initialization strategies to ensure Bootstrap is available
document.addEventListener('DOMContentLoaded', function() {
    // Strategy 1: Try immediately
    if (window.bootstrap) {
        initializeBootstrap();
    } else {
        // Strategy 2: Wait for vendor bundle to load
        let attempts = 0;
        const maxAttempts = 50; // 5 seconds max
        
        const checkBootstrap = setInterval(function() {
            attempts++;
            
            if (window.bootstrap) {
                clearInterval(checkBootstrap);
                initializeBootstrap();
            } else if (attempts >= maxAttempts) {
                clearInterval(checkBootstrap);
                console.error('Bootstrap not loaded after 5 seconds');
            }
        }, 100);
    }
});

// Strategy 3: Also try on window load as fallback
window.addEventListener('load', function() {
    if (window.bootstrap && !window.bootstrapInitialized) {
        window.bootstrapInitialized = true;
        initializeBootstrap();
    }
});

// Global library setup for legacy compatibility (these should be available from vendor bundle)
// But we'll keep these as fallbacks
if (!window.QRious && typeof require !== 'undefined') {
    try {
        window.QRious = require('qrious');
    } catch (e) {
        console.warn('QRious not available:', e.message);
    }
}
if (!window.fornac && typeof require !== 'undefined') {
    try {
        window.fornac = require('fornac');
    } catch (e) {
        console.warn('fornac not available:', e.message);
    }
}

// Modern FontAwesome implementation
import { library } from '@fortawesome/fontawesome-svg-core';
import {
  faEdit,
  faPlusCircle,
  faInfoCircle,
  faTrash,
  faArrowLeft,
  faCheck,
  faCheckCircle,
  faExclamationTriangle,
  faCircleNotch,
  faQuestionCircle,
  faFilter,
  faSortUp,
  faSortDown,
  faSort,
  faDownload,
  faUpload
} from '@fortawesome/free-solid-svg-icons';

// Add icons to the library
library.add(
  faEdit,
  faPlusCircle,
  faInfoCircle,
  faTrash,
  faArrowLeft,
  faCheck,
  faCheckCircle,
  faExclamationTriangle,
  faCircleNotch,
  faQuestionCircle,
  faFilter,
  faSortUp,
  faSortDown,
  faSort,
  faDownload,
  faUpload
);
