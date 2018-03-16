document.addEventListener("DOMContentLoaded", function() {
    var params = (new URL(document.location)).searchParams;

    if (params.has("filterParam")) {
        document.getElementById("filterOptions").classList.toggle('d-none');
        document.getElementById("filterValue").value = params.get("filterValue");
    }
    else {
        document.getElementById("showFilterOptions").classList.toggle('d-none');
    }
});

document.getElementById("filterSubmit").addEventListener("click", function() {
    var params = (new URL(document.location)).searchParams;
    var filterParam = document.getElementById("filterParam");
    var filterCondition = document.getElementById("filterCondition");
    var filterValue = document.getElementById("filterValue");
    var url = document.URL;

    if (params.has("sortOrder")) {
        var sortOrder = params.get('sortOrder');
        url = url.substring(0, url.indexOf('?'));
        url += "?sortOrder="+sortOrder+"&";
    }
    else {
        url = url.substring(0, url.indexOf('?'));
        url += "?";
    }
    url += "filterParam="+filterParam.value;
    url += "&filterCondition="+filterCondition.value;
    url += "&filterValue="+filterValue.value;

    window.location.href = url;
});

document.getElementById("refineResults").addEventListener("click", function() {
    var refineResults = document.getElementById("showFilterOptions");
    var filterOptions = document.getElementById("filterOptions");

    refineResults.classList.toggle('d-none');
    filterOptions.classList.toggle('d-none');
});