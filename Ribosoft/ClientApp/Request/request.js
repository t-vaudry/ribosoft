import { createApp, ref, onMounted, defineComponent } from 'vue';
import axios from 'axios';

const RequestApp = defineComponent({
  setup() {
    // Reactive data
    const inVivoSelected = ref(false);
    const ORFExists = ref(false);
    const cutSites = ref([]);
    const genbankLoading = ref(false);
    const genbankStatus = ref('');

    // Methods
    const expand = (title, body) => {
      const panelTitle = document.getElementById(title);
      const panelBody = document.getElementById(body);

      if (panelTitle && panelBody) {
        panelTitle.classList.toggle('collapsed');
        panelBody.classList.toggle('collapse');
      }
    };

    const targetEnvironment = () => {
      const targetTempField = document.getElementById('TargetTemperature');
      const radios = document.getElementsByName('SelectedTargetEnvironment');

      if (!targetTempField || !radios) return;

      for (let i = 0; i < radios.length; i++) {
        if (radios[i].checked && radios[i].value === 'InVivo') {
          inVivoSelected.value = true;
          targetTempField.value = 37;
        } else if (radios[i].checked) {
          inVivoSelected.value = false;
          targetTempField.value = 25;
        }
      }
    };

    const openReadingFrame = () => {
      const orfStartField = document.getElementById('orfStart');
      const orfEndField = document.getElementById('orfEnd');

      if (!orfStartField || !orfEndField) return;

      const orfStart = parseInt(orfStartField.value) || 0;
      const orfEnd = parseInt(orfEndField.value) || 0;

      ORFExists.value = orfStart > 0 && orfEnd > 0 && orfEnd > orfStart;
    };

    const processFASTAfile = (event) => {
      const file = event.target.files[0];
      if (!file) return;

      const reader = new FileReader();
      reader.onload = (e) => {
        const content = e.target.result;
        const lines = content.split('\n');
        let sequence = '';

        // Skip header lines (starting with >)
        for (let line of lines) {
          if (!line.startsWith('>') && line.trim()) {
            sequence += line.trim().toUpperCase();
          }
        }

        const sequenceField = document.getElementById('inputSequence');
        if (sequenceField) {
          sequenceField.value = sequence;
        }
      };
      reader.readAsText(file);
    };

    const getFromGenbank = async () => {
      const accessionField = document.getElementById('accesionNumber');
      if (!accessionField || !accessionField.value.trim()) return;

      genbankLoading.value = true;
      genbankStatus.value = '';

      try {
        const response = await axios.get(`/api/genbank/${accessionField.value.trim()}`);
        
        if (response.data && response.data.sequence) {
          const sequenceField = document.getElementById('inputSequence');
          if (sequenceField) {
            sequenceField.value = response.data.sequence.toUpperCase();
          }
          genbankStatus.value = '';
        } else {
          genbankStatus.value = 'Sequence not found';
        }
      } catch (error) {
        console.error('GenBank request failed:', error);
        genbankStatus.value = 'Failed to retrieve sequence';
      } finally {
        genbankLoading.value = false;
      }
    };

    // Initialize on mount
    onMounted(() => {
      // Initialize form state
      targetEnvironment();
      openReadingFrame();
    });

    return {
      inVivoSelected,
      ORFExists,
      cutSites,
      genbankLoading,
      genbankStatus,
      expand,
      targetEnvironment,
      openReadingFrame,
      processFASTAfile,
      getFromGenbank
    };
  }
});

// Mount the Vue app
const app = createApp(RequestApp);
app.mount('#app');
