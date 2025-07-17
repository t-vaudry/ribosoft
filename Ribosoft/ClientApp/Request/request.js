import axios from 'axios';

// Request functionality without Vue.js
class RequestApp {
  constructor() {
    this.inVivoSelected = false;
    this.ORFExists = false;
    this.genbankLoading = false;
    this.genbankStatus = '';
    
    this.init();
  }

  init() {
    this.bindEvents();
    this.initializeFormState();
  }

  bindEvents() {
    // FASTA file processing
    const fastaInput = document.getElementById('FASTAfileInput');
    if (fastaInput) {
      fastaInput.addEventListener('change', (e) => this.processFASTAfile(e));
    }

    // GenBank search
    const genbankBtn = document.getElementById('genbankSearchBtn');
    if (genbankBtn) {
      genbankBtn.addEventListener('click', () => this.getFromGenbank());
    }

    // ORF change handlers
    const orfStart = document.getElementById('orfStart');
    const orfEnd = document.getElementById('orfEnd');
    if (orfStart) orfStart.addEventListener('change', () => this.openReadingFrame());
    if (orfEnd) orfEnd.addEventListener('change', () => this.openReadingFrame());

    // Target environment change
    const targetEnvRadios = document.querySelectorAll('input[name="SelectedTargetEnvironment"]');
    targetEnvRadios.forEach(radio => {
      radio.addEventListener('change', () => this.targetEnvironment());
    });
  }

  initializeFormState() {
    // Initialize form state
    this.targetEnvironment();
    this.openReadingFrame();
  }

  targetEnvironment() {
    const targetTempField = document.getElementById('TargetTemperature');
    const radios = document.getElementsByName('SelectedTargetEnvironment');
    const inVivoSettings = document.getElementById('inVivoSettings');

    if (!targetTempField || !radios || !inVivoSettings) return;

    for (let i = 0; i < radios.length; i++) {
      if (radios[i].checked && radios[i].value === 'InVivo') {
        this.inVivoSelected = true;
        targetTempField.value = 37;
        inVivoSettings.style.display = 'block';
      } else if (radios[i].checked) {
        this.inVivoSelected = false;
        targetTempField.value = 25;
        inVivoSettings.style.display = 'none';
      }
    }
  }

  openReadingFrame() {
    const orfStartField = document.getElementById('orfStart');
    const orfEndField = document.getElementById('orfEnd');
    const targetRegionsCard = document.getElementById('targetRegionsCard');

    if (!orfStartField || !orfEndField || !targetRegionsCard) return;

    const orfStart = parseInt(orfStartField.value) || 0;
    const orfEnd = parseInt(orfEndField.value) || 0;

    this.ORFExists = orfStart > 0 && orfEnd > 0 && orfEnd > orfStart;
    
    if (this.ORFExists) {
      targetRegionsCard.style.display = 'block';
    } else {
      targetRegionsCard.style.display = 'none';
    }
  }

  processFASTAfile(event) {
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
  }

  async getFromGenbank() {
    const accessionField = document.getElementById('accesionNumber');
    const loadingSpan = document.getElementById('genbankLoading');
    const statusSpan = document.getElementById('genbankStatus');
    const genbankBtn = document.getElementById('genbankSearchBtn');
    
    if (!accessionField || !accessionField.value.trim()) return;

    this.genbankLoading = true;
    this.genbankStatus = '';
    
    // Update UI
    if (loadingSpan) loadingSpan.style.display = 'inline';
    if (statusSpan) statusSpan.textContent = '';
    if (genbankBtn) genbankBtn.disabled = true;
    if (accessionField) accessionField.disabled = true;

    try {
      const response = await axios.get(`/Request/GetSequenceFromGenbank?accession=${encodeURIComponent(accessionField.value.trim())}`);
      
      if (response.data && response.data.result && response.data.result.sequence) {
        const sequenceField = document.getElementById('inputSequence');
        if (sequenceField) {
          sequenceField.value = response.data.result.sequence.toUpperCase();
        }
        this.genbankStatus = '';
      } else {
        this.genbankStatus = 'Sequence not found';
      }
    } catch (error) {
      console.error('GenBank request failed:', error);
      if (error.response && error.response.status === 404) {
        this.genbankStatus = 'Sequence not found';
      } else {
        this.genbankStatus = 'Failed to retrieve sequence';
      }
    } finally {
      this.genbankLoading = false;
      
      // Update UI
      if (loadingSpan) loadingSpan.style.display = 'none';
      if (statusSpan) statusSpan.textContent = this.genbankStatus;
      if (genbankBtn) genbankBtn.disabled = false;
      if (accessionField) accessionField.disabled = false;
    }
  }
}

// Initialize the app when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
  new RequestApp();
});
