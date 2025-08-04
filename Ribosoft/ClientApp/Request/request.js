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
    this.initializeSequenceInput();
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

  initializeSequenceInput() {
    const sequenceField = document.getElementById('inputSequence');
    if (!sequenceField) return;

    // Add real-time sequence processing
    sequenceField.addEventListener('input', (e) => this.processSequenceInput(e));
    sequenceField.addEventListener('paste', (e) => this.handleSequencePaste(e));
  }

  processSequenceInput(event) {
    const field = event.target;
    const originalValue = field.value;
    
    // Remove all whitespace and convert to uppercase
    const cleanedValue = originalValue.replace(/\s/g, '').toUpperCase();
    
    // Only update if there was a change (to avoid cursor jumping)
    if (originalValue !== cleanedValue) {
      const cursorPosition = field.selectionStart;
      field.value = cleanedValue;
      
      // Restore cursor position, accounting for removed characters
      const removedChars = originalValue.length - cleanedValue.length;
      field.setSelectionRange(cursorPosition - removedChars, cursorPosition - removedChars);
      
      // Show feedback if whitespace was removed
      if (removedChars > 0) {
        this.showSequenceFeedback(`Removed ${removedChars} whitespace character${removedChars > 1 ? 's' : ''}`, 'info');
      }
    }
  }

  handleSequencePaste(event) {
    // Get pasted content
    const pastedText = (event.clipboardData || window.clipboardData).getData('text');
    
    if (pastedText) {
      // Count whitespace characters
      const whitespaceCount = (pastedText.match(/\s/g) || []).length;
      
      if (whitespaceCount > 0) {
        // Show feedback about whitespace removal
        setTimeout(() => {
          this.showSequenceFeedback(
            `Pasted sequence cleaned: removed ${whitespaceCount} whitespace character${whitespaceCount > 1 ? 's' : ''}`, 
            'success'
          );
        }, 100);
      }
    }
  }

  showSequenceFeedback(message, type = 'info') {
    // Remove any existing feedback
    const existingFeedback = document.getElementById('sequence-feedback');
    if (existingFeedback) {
      existingFeedback.remove();
    }

    // Create feedback element
    const feedback = document.createElement('div');
    feedback.id = 'sequence-feedback';
    feedback.className = `alert alert-${type} alert-dismissible fade show mt-2`;
    feedback.style.fontSize = '0.875rem';
    feedback.innerHTML = `
      <i class="fas fa-info-circle me-2"></i>${message}
      <button type="button" class="btn-close btn-close-sm" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

    // Insert after the sequence input
    const sequenceField = document.getElementById('inputSequence');
    const formGroup = sequenceField.closest('.mb-4');
    if (formGroup) {
      formGroup.appendChild(feedback);
    }

    // Auto-remove after 3 seconds
    setTimeout(() => {
      if (feedback && feedback.parentNode) {
        feedback.remove();
      }
    }, 3000);
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
        
        // Show feedback about FASTA processing
        this.showSequenceFeedback('FASTA file processed and sequence extracted', 'success');
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
          
          // Show feedback about GenBank retrieval
          this.showSequenceFeedback('Sequence retrieved from GenBank successfully', 'success');
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
