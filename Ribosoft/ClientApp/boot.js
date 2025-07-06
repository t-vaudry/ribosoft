// Import modern CSS and styles
import './css/site.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'jquery-ui-themes/themes/redmond/jquery-ui.css';
import 'jquery-ui-themes/themes/redmond/theme.css';
import 'structured-filter/css/structured-filter.css';

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
