// Site-wide JavaScript for PocketFence Dashboard

// Page Load Animation
window.addEventListener('load', function() {
    document.body.classList.add('loaded');
});

// Auto-hide alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function() {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.5s';
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 500);
        }, 5000);
    });
    
    // Add loading to all form submissions (global)
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', function(e) {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                showLoading();
            }
        });
    });
});

// Global Loading Overlay
function showLoading() {
    document.getElementById('loadingOverlay')?.classList.add('show');
}

function hideLoading() {
    document.getElementById('loadingOverlay')?.classList.remove('show');
}

// Toast Notification System
function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) return;
    
    const toastId = 'toast-' + Date.now();
    const iconMap = {
        success: 'check-circle-fill',
        error: 'exclamation-triangle-fill',
        warning: 'exclamation-circle-fill',
        info: 'info-circle-fill'
    };
    
    const bgMap = {
        success: 'bg-success',
        error: 'bg-danger',
        warning: 'bg-warning',
        info: 'bg-info'
    };
    
    const toastHtml = `
        <div class="toast align-items-center text-white ${bgMap[type]} border-0" id="${toastId}" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi bi-${iconMap[type]} me-2"></i>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, { delay: 4000 });
    toast.show();
    
    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });
}

// Form Validation Helper
function validateForm(formElement) {
    const inputs = formElement.querySelectorAll('input[required], select[required], textarea[required]');
    let isValid = true;
    let errorMessages = [];
    
    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.classList.add('is-invalid');
            isValid = false;
            
            // Create or update error message
            let errorDiv = input.nextElementSibling;
            if (!errorDiv || !errorDiv.classList.contains('invalid-feedback')) {
                errorDiv = document.createElement('div');
                errorDiv.className = 'invalid-feedback';
                input.parentNode.insertBefore(errorDiv, input.nextSibling);
            }
            
            const fieldName = input.getAttribute('placeholder') || input.getAttribute('name') || 'This field';
            errorDiv.textContent = `❌ ${fieldName} is required.`;
        } else {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
            
            // Remove error message if exists
            const errorDiv = input.nextElementSibling;
            if (errorDiv && errorDiv.classList.contains('invalid-feedback')) {
                errorDiv.remove();
            }
        }
        
        // Email validation
        if (input.type === 'email' && input.value.trim()) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(input.value)) {
                input.classList.add('is-invalid');
                input.classList.remove('is-valid');
                isValid = false;
                
                let errorDiv = input.nextElementSibling;
                if (!errorDiv || !errorDiv.classList.contains('invalid-feedback')) {
                    errorDiv = document.createElement('div');
                    errorDiv.className = 'invalid-feedback';
                    input.parentNode.insertBefore(errorDiv, input.nextSibling);
                }
                errorDiv.textContent = '❌ Please enter a valid email address.';
            }
        }
    });
    
    if (!isValid) {
        showToast('❌ Please correct the errors in the form.', 'error');
    }
    
    return isValid;
}

// Auto-clear validation states on input
document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('input, select, textarea').forEach(input => {
        input.addEventListener('input', function() {
            this.classList.remove('is-invalid', 'is-valid');
        });
    });
});

// Search functionality for blocked content
const searchBox = document.getElementById('searchBox');
if (searchBox) {
    searchBox.addEventListener('input', function(e) {
        const searchTerm = e.target.value.toLowerCase();
        const rows = document.querySelectorAll('tbody tr');
        
        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            row.style.display = text.includes(searchTerm) ? '' : 'none';
        });
    });
}

// Session Timeout Warning System
(function() {
    const SESSION_TIMEOUT_MINUTES = 30;
    const WARNING_MINUTES = 5; // Show warning 5 minutes before timeout
    
    let lastActivity = Date.now();
    let warningShown = false;
    
    // Track user activity
    document.addEventListener('mousemove', resetActivity);
    document.addEventListener('keypress', resetActivity);
    document.addEventListener('click', resetActivity);
    document.addEventListener('scroll', resetActivity);
    
    function resetActivity() {
        lastActivity = Date.now();
        warningShown = false;
        hideSessionWarning();
    }
    
    // Check session timeout every minute
    setInterval(checkSessionTimeout, 60000);
    
    function checkSessionTimeout() {
        const elapsed = (Date.now() - lastActivity) / 1000 / 60; // minutes
        const remaining = SESSION_TIMEOUT_MINUTES - elapsed;
        
        // Show warning when 5 minutes remain
        if (remaining <= WARNING_MINUTES && remaining > 0 && !warningShown) {
            showSessionWarning(remaining);
            warningShown = true;
        }
        
        // Redirect to login when session expires
        if (remaining <= 0) {
            window.location.href = '/logout';
        }
    }
    
    function showSessionWarning(minutesRemaining) {
        const toastEl = document.getElementById('sessionWarningToast');
        if (!toastEl) return;
        
        const toast = new bootstrap.Toast(toastEl, {
            autohide: false
        });
        
        // Update time every 10 seconds
        const interval = setInterval(() => {
            const elapsed = (Date.now() - lastActivity) / 1000 / 60;
            const remaining = SESSION_TIMEOUT_MINUTES - elapsed;
            
            if (remaining <= 0) {
                clearInterval(interval);
                return;
            }
            
            const mins = Math.floor(remaining);
            const secs = Math.floor((remaining - mins) * 60);
            document.getElementById('timeRemaining').textContent = `${mins}:${secs.toString().padStart(2, '0')}`;
        }, 10000);
        
        // Reset session on click
        toastEl.addEventListener('click', function() {
            resetActivity();
            clearInterval(interval);
            toast.hide();
        });
        
        toast.show();
    }
    
    function hideSessionWarning() {
        const toastEl = document.getElementById('sessionWarningToast');
        if (toastEl) {
            const toast = bootstrap.Toast.getInstance(toastEl);
            if (toast) toast.hide();
        }
    }
})();
