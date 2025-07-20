// Import jQuery for Jobs index functionality
import 'jquery/dist/jquery.js';

// Jobs index functionality
class JobsIndexApp {
  constructor() {
    this.currentJobId = null;
    this.init();
  }

  init() {
    this.bindEvents();
    this.initializeTooltips();
    this.setupFileUpload();
    this.setupAutoAlertDismiss();
  }

  bindEvents() {
    // Cancel Job Modal Handling
    this.bindCancelJobModal();
    this.bindConfirmCancelJob();
    this.bindModalReset();
    
    // Delete Job Modal Handling
    this.bindDeleteJobModal();
    this.bindConfirmDeleteJob();
    this.bindDeleteModalReset();
  }

  initializeTooltips() {
    // Initialize Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
      return new bootstrap.Tooltip(tooltipTriggerEl);
    });
  }

  setupFileUpload() {
    // File upload handling
    $('#attach-button').on('click', () => {
      $('#uploadFile').click();
    });

    $('#uploadFile').on('change', function() {
      if (this.files && this.files.length > 0) {
        $('#attach-submit').click();
      }
    });
  }

  setupAutoAlertDismiss() {
    // Auto-dismiss alerts after 5 seconds
    setTimeout(() => {
      $('.alert').fadeOut('slow');
    }, 5000);
  }

  bindCancelJobModal() {
    // When cancel button is clicked, populate modal with job details
    $('[data-bs-target="#cancelJobModal"]').on('click', (e) => {
      const $button = $(e.currentTarget);
      this.currentJobId = $button.data('job-id');
      const jobName = $button.data('job-name');
      const jobCreated = $button.data('job-created');

      $('#modal-job-id').text(this.currentJobId);
      $('#modal-job-name').text(jobName);
      $('#modal-job-created').text(new Date(jobCreated).toLocaleString());
    });
  }

  bindConfirmCancelJob() {
    // Handle the actual cancellation
    $('#confirmCancelJob').on('click', () => {
      if (!this.currentJobId) return;

      const $button = $('#confirmCancelJob');
      const $buttonText = $button.find('.button-text');
      const $spinner = $button.find('.spinner-border');

      // Show loading state
      $button.prop('disabled', true);
      $buttonText.text('Cancelling...');
      $spinner.removeClass('d-none');

      // Make AJAX request to cancel the job
      $.ajax({
        url: '/Jobs/CancelJob',
        type: 'POST',
        data: {
          id: this.currentJobId,
          __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: (response) => {
          if (response.success) {
            // Hide modal
            $('#cancelJobModal').modal('hide');
            
            // Show success message
            this.showAlert('success', 'Job cancelled successfully!', 'The job has been stopped and marked as cancelled.');
            
            // Remove the job from the in-progress list or reload page
            setTimeout(() => {
              location.reload();
            }, 1500);
          } else {
            this.showAlert('danger', 'Cancellation Failed', response.message || 'An error occurred while cancelling the job.');
          }
        },
        error: (xhr, status, error) => {
          console.error('Cancel job error:', error);
          this.showAlert('danger', 'Cancellation Failed', 'An unexpected error occurred. Please try again.');
        },
        complete: () => {
          // Reset button state
          $button.prop('disabled', false);
          $buttonText.text('Cancel Job');
          $spinner.addClass('d-none');
        }
      });
    });
  }

  bindModalReset() {
    // Reset modal when it's hidden
    $('#cancelJobModal').on('hidden.bs.modal', () => {
      this.currentJobId = null;
      $('#modal-job-id').text('-');
      $('#modal-job-name').text('-');
      $('#modal-job-created').text('-');
      
      // Reset button state
      const $button = $('#confirmCancelJob');
      $button.prop('disabled', false);
      $button.find('.button-text').text('Cancel Job');
      $button.find('.spinner-border').addClass('d-none');
    });
  }

  showAlert(type, title, message) {
    const iconClass = type === 'success' ? 'check-circle' : 'exclamation-triangle';
    const alertHtml = `
      <div class="alert alert-${type} alert-dismissible fade show shadow-sm mb-4" role="alert">
        <div class="d-flex align-items-center mb-2">
          <i class="fas fa-${iconClass} me-3 fs-4"></i>
          <div>
            <h5 class="alert-heading mb-1">${title}</h5>
            <p class="mb-0">${message}</p>
          </div>
        </div>
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
      </div>
    `;
    
    // Insert alert at the top of the jobs content
    $('.jobs-content').prepend(alertHtml);
    
    // Auto-dismiss after 5 seconds
    setTimeout(() => {
      $('.alert').first().fadeOut('slow');
    }, 5000);
  }

  bindDeleteJobModal() {
    // When delete button is clicked, populate modal with job details
    $('[data-bs-target="#deleteJobModal"]').on('click', (e) => {
      const $button = $(e.currentTarget);
      this.currentJobId = $button.data('job-id');
      const jobName = $button.data('job-name');
      const jobCreated = $button.data('job-created');
      const jobState = $button.data('job-state');

      $('#delete-modal-job-id').text(this.currentJobId);
      $('#delete-modal-job-name').text(jobName);
      $('#delete-modal-job-state').text(jobState);
      $('#delete-modal-job-created').text(new Date(jobCreated).toLocaleString());
    });
  }

  bindConfirmDeleteJob() {
    // Handle the actual deletion
    $('#confirmDeleteJob').on('click', () => {
      if (!this.currentJobId) return;

      const $button = $('#confirmDeleteJob');
      const $buttonText = $button.find('.button-text');
      const $spinner = $button.find('.spinner-border');

      // Show loading state
      $button.prop('disabled', true);
      $buttonText.text('Deleting...');
      $spinner.removeClass('d-none');

      // Make AJAX request to delete the job
      $.ajax({
        url: '/Jobs/DeleteJob',
        type: 'POST',
        data: {
          id: this.currentJobId,
          __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: (response) => {
          if (response.success) {
            // Hide modal
            $('#deleteJobModal').modal('hide');
            
            // Show success message
            this.showAlert('success', 'Job deleted successfully!', 'The job and all its results have been permanently deleted.');
            
            // Remove the job from the list or reload page
            setTimeout(() => {
              location.reload();
            }, 1500);
          } else {
            this.showAlert('danger', 'Deletion Failed', response.message || 'An error occurred while deleting the job.');
          }
        },
        error: (xhr, status, error) => {
          console.error('Delete job error:', error);
          this.showAlert('danger', 'Deletion Failed', 'An unexpected error occurred. Please try again.');
        },
        complete: () => {
          // Reset button state
          $button.prop('disabled', false);
          $buttonText.text('Delete Job');
          $spinner.addClass('d-none');
        }
      });
    });
  }

  bindDeleteModalReset() {
    // Reset delete modal when it's hidden
    $('#deleteJobModal').on('hidden.bs.modal', () => {
      this.currentJobId = null;
      $('#delete-modal-job-id').text('-');
      $('#delete-modal-job-name').text('-');
      $('#delete-modal-job-state').text('-');
      $('#delete-modal-job-created').text('-');
      
      // Reset button state
      const $button = $('#confirmDeleteJob');
      $button.prop('disabled', false);
      $button.find('.button-text').text('Delete Job');
      $button.find('.spinner-border').addClass('d-none');
    });
  }
}

// Initialize the app when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
  new JobsIndexApp();
});

// Export for potential use in other modules
export default JobsIndexApp;
