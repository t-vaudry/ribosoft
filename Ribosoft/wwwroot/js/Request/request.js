var app = new Vue({
    el: "#app",
    data: function() {
        return {
            message: "Hello Vue",
            ribozymes: [
                "Hammerhead",
                "Pistol"
            ],
            targetRegions: [
                "5'UTR",
                "Open Reading Frame (ORF)",
                "3'UTR"
            ],
            targetEnvironmentMethods: [
                "In-vitro",
                "In-vivo"
            ],
            targetEnvironments: [
                "Mouse",
                "Human"
            ],
            targetEnvironmentVariables: [
                {
                    id: "temperatureVariable",
                    variable: "Temperature",
                    unit: "℃"
                },
                {
                    id: "NaVariable",
                    variable: "Na",
                    unit: "nM"
                },
                {
                    id: "MgVariable",
                    variable: "Mg",
                    unit: "nM"
                },
                {
                    id: "oligomerVariable",
                    variable: "Oligomer",
                    unit: "nM"
                },
            ],
            cutSites: [
            ],
            specificityMethods: [
                "Cleavage",
                "Cleavage and Hybridization"
            ]
        }
    },
    methods: {
        expand: function(title, body) {
            panelTitle = document.getElementById(title);
            panelBody = document.getElementById(body);

            panelTitle.classList.toggle("collapsed");
            panelBody.classList.toggle("collapse");
        },
        submit: function() {

        }
    }
});