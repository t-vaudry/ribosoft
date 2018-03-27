import Vue from 'vue';
import VueResource from 'vue-resource';

Vue.use(VueResource);

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
            ],
            genbankLoading: false,
            genbankStatus: "",
        }
    },
    methods: {
        expand: function(title, body) {
            var panelTitle = document.getElementById(title);
            var panelBody = document.getElementById(body);

            panelTitle.classList.toggle("collapsed");
            panelBody.classList.toggle("collapse");
        },
        targetEnvironment: function() {
            var environment = document.getElementById("targetEnvironmentMethod");
            var radios = document.getElementsByName("SelectedTargetEnvironment");

            for (var i = 0; i < radios.length; i++) {
                if (radios[i].checked && radios[i].value == "InVivo") {
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

                inputField.value = output.replace(/T/g, "U");
            };

            reader.readAsText(file)
        },
        getFromGenbank: function() {
            let accesionField = document.getElementById('accesionNumber');
            let sequenceInputField = document.getElementById("inputSequence");
            let startIndexField = document.getElementById("OpenReadingFrameStart");
            let endIndexField = document.getElementById("OpenReadingFrameEnd");

            if (!accesionField.value) {
                return;
            } 

            let seqRoute = '/Request/GetSequenceFromGenbank?accession=' + accesionField.value;
            
            this.genbankLoading = true;

            this.$http.get(seqRoute).then(response => {
                this.genbankLoading = false;

                if ('error' in response.body) {
                    this.genbankStatus = response.body.error;
                } else if ('result' in response.body) {
                    sequenceInputField.value = response.body.result.sequence;
                    startIndexField.value = response.body.result.openReadingFrameStart;
                    endIndexField.value = response.body.result.openReadingFrameEnd;
                    accesionField.value = "";
                    this.genbankStatus = "";
                } else {
                    this.genbankStatus = "An error occurred making the request.";
                }
            }, response => {
                // error callback
                this.genbankLoading = false;
                this.genbankStatus = "An error occurred making the request.";
            });
        }
    },
    created: function () {
        this.targetEnvironment();
    }
});