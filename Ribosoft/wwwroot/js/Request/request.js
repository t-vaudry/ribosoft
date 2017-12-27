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
        processFASTAfile: function() { 
            var inputField = document.getElementById("inputSequence");
            var file = document.getElementById("FASTAfileInput").files[0];
            var reader = new FileReader();

            reader.onload = function(e) {
                var firstSequence = false;
                var text = reader.result;
                var lines = text.split('\n');
                var output = "";

                for (var i = 0; i < lines.length; i++) {
                    var line = lines[i];

                    // Check if line starts with > (start of a new sequence)
                    if (line.match(/^>/) && !firstSequence) {
                        firstSequence = true;
                    }
                    else if (line.match(/^>/) && firstSequence) {
                        // Start of 2nd sequence, stop processing
                        break;
                    }
                    // Ignore blank lines
                    else if (!line.match(/^\n/)) {
                        output += line;
                    }
                }

                inputField.value = output;
            }

            reader.readAsText(file)
        },
        submit: function() {
            // TODO: Main vailidation driver
        }
    }
});