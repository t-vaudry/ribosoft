import { createApp, ref, onMounted } from 'vue';
import axios from 'axios';
import vSelect from 'vue-select';
import 'vue-select/dist/vue-select.css';

const { defineComponent } = Vue;

const RequestApp = defineComponent({
    components: {
        'v-select': vSelect
    },
    setup() {
        // Reactive data
        const options = ref([]);
        const inVivoOptions = ref([]);
        const inVivoSelected = ref(false);
        const ORFExists = ref(false);
        const cutSites = ref([]);
        const genbankLoading = ref(false);
        const genbankStatus = ref('');

        // Initialize data from DOM
        const initializeData = () => {
            // Initialize ribozyme options
            const ribozymeDataList = document.getElementById("ribozymeList");
            if (ribozymeDataList) {
                const ribozymeOptionsList = ribozymeDataList.children;
                const ribozymeOptionsArr = Array.from(ribozymeOptionsList).map(option => option.value);
                options.value = ribozymeOptionsArr;
            }

            // Initialize environment options
            const environmentDataList = document.getElementById("environmentList");
            if (environmentDataList) {
                const environmentOptionsList = environmentDataList.children;
                const vivoArr = Array.from(environmentOptionsList).map(option => option.value);
                inVivoOptions.value = vivoArr;
            }
        };

        // Methods
        const expand = (title, body) => {
            const panelTitle = document.getElementById(title);
            const panelBody = document.getElementById(body);

            if (panelTitle && panelBody) {
                panelTitle.classList.toggle("collapsed");
                panelBody.classList.toggle("collapse");
            }
        };

        const ribozymeId = (value) => {
            const dataList = document.getElementById("ribozymeList");
            const hiddenInput = document.getElementById("RibozymeStructure");
            
            if (!dataList || !hiddenInput) return;

            const optionsList = dataList.children;

            for (let i = 0; i < optionsList.length; i++) {
                if (optionsList[i].value === value) {
                    hiddenInput.value = optionsList[i].getAttribute('data-value');
                    return;
                }
            }

            if (value == null) {
                hiddenInput.value = -1;
            }
        };

        const vivoEnvironment = (value) => {
            const dataList = document.getElementById("environmentList");
            const hiddenInput = document.getElementById("InVivoEnvironment");
            
            if (!dataList || !hiddenInput) return;

            const optionsList = dataList.children;

            for (let i = 0; i < optionsList.length; i++) {
                if (optionsList[i].value === value) {
                    hiddenInput.value = optionsList[i].getAttribute('data-value');
                    return;
                }
            }

            if (value == null) {
                hiddenInput.value = -1;
            }
        };

        const targetEnvironment = () => {
            const targetTempField = document.getElementById("TargetTemperature");
            const radios = document.getElementsByName("SelectedTargetEnvironment");

            if (!targetTempField || !radios) return;

            for (let i = 0; i < radios.length; i++) {
                if (radios[i].checked && radios[i].value === "InVivo") {
                    inVivoSelected.value = true;
                    targetTempField.value = 37;
                } else if (radios[i].checked) {
                    inVivoSelected.value = false;
                    targetTempField.value = 22;
                }
            }
        };

        const openReadingFrame = () => {
            const orfStart = document.getElementById("orfStart")?.value || 0;
            const orfEnd = document.getElementById("orfEnd")?.value || 0;
            ORFExists.value = (orfEnd - orfStart) > 0;
        };

        const processFASTAfile = () => {
            const inputField = document.getElementById("inputSequence");
            const fileInput = document.getElementById("FASTAfileInput");
            
            if (!inputField || !fileInput || !fileInput.files[0]) return;

            const file = fileInput.files[0];
            const reader = new FileReader();

            reader.onload = (e) => {
                let firstSequence = false;
                const text = reader.result;
                const lines = text.split('\n');
                let output = "";

                for (let i = 0; i < lines.length; i++) {
                    const line = lines[i];

                    // Check if line starts with > (start of a new sequence)
                    if (line.match(/^>/) && !firstSequence) {
                        firstSequence = true;
                    } else if (line.match(/^>/) && firstSequence) {
                        // Start of 2nd sequence, stop processing
                        break;
                    } else if (!line.match(/^\n/)) {
                        // Ignore blank lines
                        output += line;
                    }
                }

                inputField.value = output.replace(/T/g, "U");
            };

            reader.readAsText(file);
        };

        const getFromGenbank = async () => {
            const accessionField = document.getElementById('accesionNumber');
            const sequenceInputField = document.getElementById("inputSequence");
            const startIndexField = document.getElementById("OpenReadingFrameStart");
            const endIndexField = document.getElementById("OpenReadingFrameEnd");

            if (!accessionField?.value) {
                return;
            }

            const seqRoute = `/Request/GetSequenceFromGenbank?accession=${accessionField.value}`;
            
            genbankLoading.value = true;
            genbankStatus.value = '';

            try {
                const response = await axios.get(seqRoute);
                genbankLoading.value = false;

                if (response.data.error) {
                    genbankStatus.value = response.data.error;
                } else if (response.data.result) {
                    if (sequenceInputField) sequenceInputField.value = response.data.result.sequence;
                    if (startIndexField) startIndexField.value = response.data.result.openReadingFrameStart;
                    if (endIndexField) endIndexField.value = response.data.result.openReadingFrameEnd;
                    if (accessionField) accessionField.value = "";
                    genbankStatus.value = "";
                } else {
                    genbankStatus.value = "An error occurred making the request.";
                }
            } catch (error) {
                genbankLoading.value = false;
                genbankStatus.value = "An error occurred making the request.";
                console.error('Genbank request error:', error);
            }
        };

        // Lifecycle
        onMounted(() => {
            initializeData();
            targetEnvironment();
        });

        // Return reactive data and methods for template
        return {
            options,
            inVivoOptions,
            inVivoSelected,
            ORFExists,
            cutSites,
            genbankLoading,
            genbankStatus,
            expand,
            ribozymeId,
            vivoEnvironment,
            targetEnvironment,
            openReadingFrame,
            processFASTAfile,
            getFromGenbank
        };
    }
});

// Create and mount the Vue 3 app
const app = createApp(RequestApp);
app.mount('#app');