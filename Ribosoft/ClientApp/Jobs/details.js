// Import jQuery and jQuery UI components
import 'jquery/dist/jquery.js';
import 'jquery-ui/ui/widget.js';
import 'jquery-ui/ui/effects/effect-highlight.js';
import 'jquery-ui/ui/widgets/button.js';
import 'jquery-ui/ui/widgets/datepicker.js';
import 'structured-filter/js/structured-filter.js';

// Jobs details functionality without Vue.js
class JobsDetailsApp {
  constructor() {
    this.init();
  }

  init() {
    this.bindEvents();
  }

  bindEvents() {
    // Make refineResults available globally for onclick handlers
    window.refineResults = () => this.refineResults();
  }

  refineResults() {
    // Toggle visibility of filter elements
    const elements = [
      { selector: '.evo-bNew', toggle: 'd-none' },
      { selector: '.evo-editFilter', toggle: 'd-none' },
      { selector: '.evo-bAdd', toggle: 'd-none' },
      { selector: '.evo-bDel', toggle: 'd-none' }
    ];

    elements.forEach(({ selector, toggle }) => {
      const element = document.querySelector(selector);
      if (element) {
        element.classList.toggle(toggle);
      }
    });

    // Toggle filter options styling
    const filterOptions = document.getElementById('filterOptions');
    if (filterOptions) {
      filterOptions.classList.toggle('border-0');
      filterOptions.classList.toggle('no-height');
    }

    // Toggle search filters
    const searchFilters = document.querySelector('.evo-searchFilters');
    if (searchFilters) {
      Array.from(searchFilters.children).forEach(el => {
        if (el.lastChild) {
          el.lastChild.classList.toggle('d-none');
        }
        el.classList.toggle('disabled-link');
      });
    }
  }
}

// Initialize the app when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
  new JobsDetailsApp();
});
