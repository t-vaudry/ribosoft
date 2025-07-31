// Vendor bundle entry point - exposes all vendor libraries globally

// Import jQuery and ensure global exposure
import $ from 'jquery';

// Force global exposure
window.$ = $;
window.jQuery = $;

// Also expose on global object for Node-like environments
if (typeof global !== 'undefined') {
    global.$ = $;
    global.jQuery = $;
}

console.log('jQuery exposed globally:', {
    'window.$': !!window.$,
    'window.jQuery': !!window.jQuery,
    'version': $.fn.jquery
});

// Import Bootstrap and expose globally
import * as bootstrap from 'bootstrap';
window.bootstrap = bootstrap;

console.log('Bootstrap exposed globally:', {
    'window.bootstrap': !!window.bootstrap,
    'version': bootstrap.Tooltip?.VERSION || 'unknown'
});

// Import other libraries and expose globally
import QRious from 'qrious';
import axios from 'axios';

window.QRious = QRious;
window.axios = axios;

// Import vendor CSS
import 'bootstrap/dist/css/bootstrap.min.css';
import '@fortawesome/fontawesome-free/css/all.css';

console.log('All vendor libraries loaded and exposed globally');
