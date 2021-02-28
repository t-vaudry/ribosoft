import './css/site.css';
import 'bootstrap';

require('jquery-ui-themes/themes/redmond/jquery-ui.css');
require('jquery-ui-themes/themes/redmond/theme.css');
require('structured-filter/css/structured-filter.css');

window.QRious = require('qrious');

import fontawesome from '@fortawesome/fontawesome';
import { faEdit, faPlusCircle, faInfoCircle, faTrash, faArrowLeft, faCheck, faCheckCircle, faExclamationTriangle, faCircleNotch, faQuestionCircle, faFilter, faSortUp, faSortDown, faSort, faDownload, faUpload } from '@fortawesome/fontawesome-free-solid'

fontawesome.library.add(faEdit, faPlusCircle, faInfoCircle, faTrash, faArrowLeft, faCheck, faCheckCircle, faExclamationTriangle, faCircleNotch, faQuestionCircle, faFilter, faSortUp, faSortDown, faSort, faDownload, faUpload);
