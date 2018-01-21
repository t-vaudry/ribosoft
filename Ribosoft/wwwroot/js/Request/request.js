var app = new Vue({
    el: "#app",
    data: function() {
        return {
            ribozymes: [
                "Hammerhead",
                "Pistol"
            ],
            targetEnvironments: [
                "Mouse",
                "Human"
            ],
            inVivoSelected: false,
            cutSites: [
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
        targetEnvironment: function() {
            var environment = document.getElementById("targetEnvironmentMethod");
            var radios = document.getElementsByName("TargetEnvironment.TargetEnvironment");

            for (var i = 0; i < radios.length; i++) {
                if (radios[i].checked && radios[i].value == "In-vivo") {
                    this.inVivoSelected = true;
                }
                else this.inVivoSelected = false;
            }
            
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

                inputField.value = output.replace(/T/g, "U");;
            }

            reader.readAsText(file)
        },
        getFromGenbank: function() {
            var accesionField = document.getElementById('accesionNumber');
            var sequenceInputField = document.getElementById("inputSequence");
            var route = '/Request/GetSequenceFromGenbank?accession='+accesionField.value;

            this.$http.get(route).then((response)=>{
                sequenceInputField.value = response.body;
                accesionField.value = "";
            });
        }
    }
});