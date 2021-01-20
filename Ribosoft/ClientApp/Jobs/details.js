import Vue from 'vue';
import { Dropdown } from 'bootstrap-vue/es/components';

Vue.use(Dropdown);

var app = new Vue({
    el: "#app",
    methods: {
        filterSubmit: function () {
            var params = (new URL(document.location)).searchParams;
            var filterParam = document.getElementById("FilterParam");
            var filterCondition = document.getElementById("FilterCondition");
            var filterValue = document.getElementById("FilterValue");
            var url = document.URL;

            if (params.has("sortOrder")) {
                var sortOrder = params.get('sortOrder');
                url = url.substring(0, url.indexOf('?'));
                url += "?sortOrder=" + sortOrder + "&";
            }
            else {
                url = url.substring(0, url.indexOf('?'));
                url += "?";
            }
            url += "filterParam=" + filterParam.value;
            url += "&filterCondition=" + filterCondition.value;
            url += "&filterValue=" + filterValue.value;

            var errorElement = document.getElementById("filterError");

            if (filterValue.value != "" && !isNaN(filterValue.value)) {
                if (!errorElement.classList.contains("d-none")) {
                    errorElement.classList.toggle('d-none');
                }

                window.location.href = url;
            } else {
                if (errorElement.classList.contains("d-none")) {
                    errorElement.classList.toggle('d-none');
                }
            }
        },
        refineResults: function () {
            var refineResults = document.getElementById("showFilterOptions");
            var filterOptions = document.getElementById("filterOptions");

            refineResults.classList.toggle('d-none');
            filterOptions.classList.toggle('d-none');
        }
    }
});

document.addEventListener("DOMContentLoaded", function() {
    var params = (new URL(document.location)).searchParams;

    if (params.has("filterParam")) {
        document.getElementById("filterOptions").classList.toggle('d-none');
    }
    else {
        document.getElementById("showFilterOptions").classList.toggle('d-none');
    }
});