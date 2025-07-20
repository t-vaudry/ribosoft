// Import modern CSS and styles
import './css/site.scss';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'jquery-ui-themes/themes/redmond/jquery-ui.css';
import 'jquery-ui-themes/themes/redmond/theme.css';
import 'structured-filter/css/structured-filter.css';

// Import and expose jQuery globally
import $ from 'jquery';
window.$ = window.jQuery = $;

// Import and expose Bootstrap 5 globally
import * as bootstrap from 'bootstrap';
window.bootstrap = bootstrap;

// Initialize Bootstrap 5 dropdowns when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Initialize all Bootstrap 5 dropdowns
    const dropdownElementList = [].slice.call(document.querySelectorAll('[data-bs-toggle="dropdown"]'));
    const dropdownList = dropdownElementList.map(function (dropdownToggleEl) {
        return new bootstrap.Dropdown(dropdownToggleEl);
    });
    console.log('Bootstrap 5 initialized with', dropdownList.length, 'dropdowns');
});

// Global library setup for legacy compatibility
window.QRious = require('qrious');
window.fornac = require('fornac');

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
