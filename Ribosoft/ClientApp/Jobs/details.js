import Vue from 'vue';
import { Dropdown } from 'bootstrap-vue/es/components';

require('jquery/dist/jquery.js');
require('jquery-ui/ui/core.js');
require('jquery-ui/ui/widget.js');
require('jquery-ui/ui/effects/effect-highlight.js');
require('jquery-ui/ui/widgets/button.js');
require('jquery-ui/ui/widgets/datepicker.js');
require('structured-filter/js/structured-filter.js');

Vue.use(Dropdown);

var app = new Vue({
    el: "#app",
    methods: {
        refineResults: function () {
            var newFilter = document.getElementsByClassName('evo-bNew')[0];
            newFilter.classList.toggle('d-none');

            var editFilter = document.getElementsByClassName('evo-editFilter')[0];
            editFilter.classList.toggle('d-none');

            var addFilter = document.getElementsByClassName('evo-bAdd')[0];
            addFilter.classList.toggle('d-none');

            var delFilter = document.getElementsByClassName('evo-bDel')[0];
            delFilter.classList.toggle('d-none');

            var filterOptions = document.getElementById('filterOptions');
            filterOptions.classList.toggle('border-0');
            filterOptions.classList.toggle('no-height');

            var searchFilters = document.getElementsByClassName('evo-searchFilters')[0];
            Array.from(searchFilters.children).forEach(function (el) {
                el.lastChild.classList.toggle('d-none');
                el.classList.toggle('disabled-link');
            });
        }
    }
});