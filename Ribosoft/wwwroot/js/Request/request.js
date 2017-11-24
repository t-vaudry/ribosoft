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
            specificityMethods: [
                "Cleavage",
                "Cleavage and Hybridization"
            ]
        }
    }
});