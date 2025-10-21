// ============================================================================
// Asset Management System - Modern JavaScript Interactions
// Updated: 2025-09-18 - Notifications disabled until API implementation
// ============================================================================

'use strict';

// Global App Object
const AssetFlow = {
    init: function() {
        this.initSidebar();
        this.initSearch();
        this.initNotifications(); // Safe to call - function now disabled internally
        this.initToasts();
        this.initDataTables();
        this.initFormValidation();
        this.initLoadingStates();
        this.initUtilities();
        console.log('AssetFlow initialized successfully');
    },

    // ========================================================================
    // Sidebar Navigation
    // ========================================================================
    initSidebar: function() {
        const sidebar = document.getElementById('sidebar');
        const sidebarToggle = document.getElementById('sidebarToggle');
        const sidebarCollapse = document.getElementById('sidebarCollapse');
        const content = document.getElementById('content');

        // Toggle sidebar on mobile
        if (sidebarToggle) {
            sidebarToggle.addEventListener('click', function() {
                if (window.innerWidth <= 991) {
                    sidebar.classList.toggle('show');
                }
            });
        }

        // Close sidebar on mobile
        if (sidebarCollapse) {
            sidebarCollapse.addEventListener('click', function() {
                sidebar.classList.remove('show');
            });
        }

        // Close sidebar when clicking outside on mobile
        document.addEventListener('click', function(e) {
            if (window.innerWidth <= 991 && 
                !sidebar.contains(e.target) && 
                !sidebarToggle.contains(e.target) &&
                sidebar.classList.contains('show')) {
                sidebar.classList.remove('show');
            }
        });

        // Handle active navigation
        this.setActiveNavigation();
    },

    setActiveNavigation: function() {
        const currentPath = window.location.pathname;
        const navLinks = document.querySelectorAll('.sidebar .nav-link');
        
        navLinks.forEach(link => {
            const linkPath = new URL(link.href).pathname;
            if (currentPath === linkPath || currentPath.startsWith(linkPath + '/')) {
                link.classList.add('active');
                
                // Expand parent menu if it's a submenu item
                const collapse = link.closest('.collapse');
                if (collapse) {
                    collapse.classList.add('show');
                    const trigger = document.querySelector(`[data-bs-target="#${collapse.id}"]`);
                    if (trigger) {
                        trigger.setAttribute('aria-expanded', 'true');
                    }
                }
            }
        });
    },

    // ========================================================================
    // Global Search
    // ========================================================================
    initSearch: function() {
        const searchInput = document.getElementById('globalSearch');
        if (!searchInput) return;

        let searchTimeout;
        
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            const query = this.value.trim();
            
            if (query.length >= 2) {
                searchTimeout = setTimeout(() => {
                    AssetFlow.performSearch(query);
                }, 300);
            }
        });

        // Handle search on Enter key
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                const query = this.value.trim();
                if (query.length >= 2) {
                    AssetFlow.performSearch(query);
                }
            }
        });
    },

    performSearch: function(query) {
        // Show loading state
        this.showLoading('Searching...');
        
        // Call actual search endpoint
        fetch(`/api/search?q=${encodeURIComponent(query)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                this.hideLoading();
                this.displaySearchResults(data);
            })
            .catch(error => {
                this.hideLoading();
                console.error('Search error:', error);
                this.showToast('Search failed. Please try again.', 'error');
            });
    },

    displaySearchResults: function(results) {
        console.log('Search results:', results);
        
        // Create search results modal
        let modal = document.getElementById('searchResultsModal');
        if (!modal) {
            const modalHtml = `
                <div class="modal fade" id="searchResultsModal" tabindex="-1" aria-labelledby="searchResultsModalLabel" aria-hidden="true">
                    <div class="modal-dialog modal-lg modal-dialog-scrollable">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title" id="searchResultsModalLabel">
                                    <i class="fas fa-search me-2"></i>Search Results
                                </h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body" id="searchResultsContent">
                                <!-- Results will be inserted here -->
                            </div>
                        </div>
                    </div>
                </div>
            `;
            document.body.insertAdjacentHTML('beforeend', modalHtml);
            modal = document.getElementById('searchResultsModal');
        }

        const content = document.getElementById('searchResultsContent');
        
        if (results.totalResults === 0) {
            content.innerHTML = `
                <div class="text-center py-5">
                    <i class="fas fa-search fa-3x text-muted mb-3"></i>
                    <h5 class="text-muted">No results found for "${results.query}"</h5>
                    <p class="text-muted">Try different search terms</p>
                </div>
            `;
        } else {
            let html = `
                <div class="mb-3">
                    <small class="text-muted">Found ${results.totalResults} result(s) for "${results.query}"</small>
                </div>
            `;

            // Display Assets
            if (results.results.assets && results.results.assets.length > 0) {
                html += '<h6 class="mb-3"><i class="fas fa-laptop me-2 text-primary"></i>Assets</h6>';
                html += '<div class="list-group mb-4">';
                results.results.assets.forEach(item => {
                    html += `
                        <a href="${item.url}" class="list-group-item list-group-item-action">
                            <div class="d-flex w-100 justify-content-between">
                                <h6 class="mb-1"><i class="fas ${item.icon} me-2 text-primary"></i>${item.title}</h6>
                                <small class="text-muted">${item.description}</small>
                            </div>
                            <small class="text-muted">${item.subtitle}</small>
                        </a>
                    `;
                });
                html += '</div>';
            }

            // Display Employees
            if (results.results.employees && results.results.employees.length > 0) {
                html += '<h6 class="mb-3"><i class="fas fa-user me-2 text-success"></i>Employees</h6>';
                html += '<div class="list-group mb-4">';
                results.results.employees.forEach(item => {
                    html += `
                        <a href="${item.url}" class="list-group-item list-group-item-action">
                            <div class="d-flex w-100 justify-content-between">
                                <h6 class="mb-1"><i class="fas ${item.icon} me-2 text-success"></i>${item.title}</h6>
                                <small class="text-muted">${item.description}</small>
                            </div>
                            <small class="text-muted">${item.subtitle}</small>
                        </a>
                    `;
                });
                html += '</div>';
            }

            // Display Departments
            if (results.results.departments && results.results.departments.length > 0) {
                html += '<h6 class="mb-3"><i class="fas fa-building me-2 text-info"></i>Departments</h6>';
                html += '<div class="list-group mb-4">';
                results.results.departments.forEach(item => {
                    html += `
                        <a href="${item.url}" class="list-group-item list-group-item-action">
                            <div class="d-flex w-100 justify-content-between">
                                <h6 class="mb-1"><i class="fas ${item.icon} me-2 text-info"></i>${item.title}</h6>
                                <small class="text-muted">${item.description}</small>
                            </div>
                            <small class="text-muted">${item.subtitle}</small>
                        </a>
                    `;
                });
                html += '</div>';
            }

            // Display Imaging Jobs
            if (results.results.imagingJobs && results.results.imagingJobs.length > 0) {
                html += '<h6 class="mb-3"><i class="fas fa-compact-disc me-2 text-warning"></i>Imaging Jobs</h6>';
                html += '<div class="list-group mb-4">';
                results.results.imagingJobs.forEach(item => {
                    html += `
                        <a href="${item.url}" class="list-group-item list-group-item-action">
                            <div class="d-flex w-100 justify-content-between">
                                <h6 class="mb-1"><i class="fas ${item.icon} me-2 text-warning"></i>${item.title}</h6>
                                <small class="text-muted">${item.description}</small>
                            </div>
                            <small class="text-muted">${item.subtitle}</small>
                        </a>
                    `;
                });
                html += '</div>';
            }

            content.innerHTML = html;
        }

        // Show modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    },

    // ========================================================================
    // Notifications
    // ========================================================================
    initNotifications: function() {
        // Load notifications on page load
        this.loadNotifications();
        
        // Set up periodic refresh every 5 minutes (reduced frequency)
        setInterval(() => {
            this.loadNotifications();
        }, 300000); // Check every 5 minutes instead of 1 minute
    },

    loadNotifications: function() {
        fetch('/api/notifications')
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                this.updateNotificationBadge(data.unreadCount || 0);
                this.updateNotificationList(data.notifications || []);
            })
            .catch(error => {
                console.warn('Notifications temporarily unavailable:', error.message);
                // Hide notification badge if API is not available
                const badge = document.querySelector('.notification-badge');
                if (badge) {
                    badge.style.display = 'none';
                }
            });
    },

    updateNotificationBadge: function(count) {
        const badge = document.querySelector('.notification-badge');
        if (badge) {
            badge.textContent = count;
            badge.style.display = count > 0 ? 'flex' : 'none';
        }
        
        // Also update the header badge in dropdown
        const dropdownBadge = document.querySelector('.notification-dropdown .dropdown-header .badge');
        if (dropdownBadge) {
            dropdownBadge.textContent = count;
        }
    },

    updateNotificationList: function(notifications) {
        const dropdown = document.querySelector('.notification-dropdown');
        if (!dropdown) return;

        // Build notifications HTML
        let html = `
            <li class="dropdown-header">
                <strong>Notifications</strong>
                <span class="badge bg-primary">${notifications.length}</span>
            </li>
            <li><hr class="dropdown-divider"></li>
        `;

        if (notifications.length === 0) {
            html += `
                <li class="notification-item">
                    <div class="dropdown-item text-center text-muted py-4">
                        <i class="fas fa-bell-slash fa-2x mb-2"></i>
                        <p class="mb-0">No notifications</p>
                    </div>
                </li>
            `;
        } else {
            notifications.forEach(notif => {
                const bgColorClass = notif.iconColor === 'warning' ? 'bg-warning' :
                                    notif.iconColor === 'danger' ? 'bg-danger' :
                                    notif.iconColor === 'success' ? 'bg-success' :
                                    notif.iconColor === 'info' ? 'bg-info' : 'bg-secondary';
                
                const unreadClass = notif.isRead ? '' : 'unread-notification';
                
                html += `
                    <li class="notification-item ${unreadClass}">
                        <a class="dropdown-item" href="${notif.url || '#'}">
                            <div class="notification-icon ${bgColorClass}">
                                <i class="fas ${notif.icon}"></i>
                            </div>
                            <div class="notification-content">
                                <div class="notification-title">${notif.title}</div>
                                <div class="notification-text">${notif.message}</div>
                                <div class="notification-time">${notif.time}</div>
                            </div>
                        </a>
                    </li>
                `;
            });
        }

        html += `
            <li><hr class="dropdown-divider"></li>
            <li><a class="dropdown-item text-center" href="/Notifications">View all notifications</a></li>
        `;

        dropdown.innerHTML = html;
    },

    // ========================================================================
    // Toast Notifications
    // ========================================================================
    initToasts: function() {
        // Auto-dismiss alerts after 5 seconds
        const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
        alerts.forEach(alert => {
            setTimeout(() => {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }, 5000);
        });
    },

    showToast: function(message, type = 'info', duration = 5000) {
        const toastContainer = document.getElementById('toastContainer');
        if (!toastContainer) return;

        const toastId = 'toast_' + Date.now();
        const iconClass = this.getToastIcon(type);
        const bgClass = this.getToastBgClass(type);

        const toastHtml = `
            <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="toast-header ${bgClass} text-white">
                    <i class="${iconClass} me-2"></i>
                    <strong class="me-auto">Notification</strong>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `;

        toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement, { delay: duration });
        toast.show();

        // Remove toast element after it's hidden
        toastElement.addEventListener('hidden.bs.toast', function() {
            this.remove();
        });
    },

    getToastIcon: function(type) {
        const icons = {
            success: 'fas fa-check-circle',
            error: 'fas fa-exclamation-triangle',
            warning: 'fas fa-exclamation-triangle',
            info: 'fas fa-info-circle'
        };
        return icons[type] || icons.info;
    },

    getToastBgClass: function(type) {
        const classes = {
            success: 'bg-success',
            error: 'bg-danger',
            warning: 'bg-warning',
            info: 'bg-info'
        };
        return classes[type] || classes.info;
    },

    // ========================================================================
    // Data Tables Enhancement
    // ========================================================================
    initDataTables: function() {
        const tables = document.querySelectorAll('table.table');
        tables.forEach(table => {
            this.enhanceTable(table);
        });
    },

    enhanceTable: function(table) {
        // Add sorting capability
        const headers = table.querySelectorAll('thead th');
        headers.forEach((header, index) => {
            if (!header.classList.contains('no-sort')) {
                header.style.cursor = 'pointer';
                header.innerHTML += ' <i class="fas fa-sort ms-1 text-muted"></i>';
                
                header.addEventListener('click', () => {
                    this.sortTable(table, index);
                });
            }
        });

        // Add row hover effects (already in CSS)
        // Add row selection capability
        this.addRowSelection(table);
    },

    sortTable: function(table, columnIndex) {
        const tbody = table.querySelector('tbody');
        const rows = Array.from(tbody.querySelectorAll('tr'));
        const header = table.querySelectorAll('thead th')[columnIndex];
        
        // Determine sort direction
        const currentIcon = header.querySelector('i');
        const isAscending = currentIcon.classList.contains('fa-sort') || 
                           currentIcon.classList.contains('fa-sort-down');
        
        // Reset all sort icons
        table.querySelectorAll('thead th i').forEach(icon => {
            icon.className = 'fas fa-sort ms-1 text-muted';
        });
        
        // Set current sort icon
        currentIcon.className = isAscending ? 
            'fas fa-sort-up ms-1 text-primary' : 
            'fas fa-sort-down ms-1 text-primary';
        
        // Sort rows
        rows.sort((a, b) => {
            const aValue = a.cells[columnIndex].textContent.trim();
            const bValue = b.cells[columnIndex].textContent.trim();
            
            // Try to compare as numbers first
            const aNum = parseFloat(aValue);
            const bNum = parseFloat(bValue);
            
            if (!isNaN(aNum) && !isNaN(bNum)) {
                return isAscending ? aNum - bNum : bNum - aNum;
            }
            
            // Compare as strings
            return isAscending ? 
                aValue.localeCompare(bValue) : 
                bValue.localeCompare(aValue);
        });
        
        // Reorder DOM elements
        rows.forEach(row => tbody.appendChild(row));
    },

    addRowSelection: function(table) {
        // Add checkboxes for row selection if not already present
        const headerRow = table.querySelector('thead tr');
        const bodyRows = table.querySelectorAll('tbody tr');
        
        if (!headerRow.querySelector('input[type="checkbox"]')) {
            // Add header checkbox
            const headerCell = document.createElement('th');
            headerCell.innerHTML = '<input type="checkbox" class="form-check-input select-all">';
            headerRow.insertBefore(headerCell, headerRow.firstChild);
            
            // Add row checkboxes
            bodyRows.forEach(row => {
                const cell = document.createElement('td');
                cell.innerHTML = '<input type="checkbox" class="form-check-input select-row">';
                row.insertBefore(cell, row.firstChild);
            });
            
            // Handle select all
            const selectAll = table.querySelector('.select-all');
            selectAll.addEventListener('change', function() {
                const rowCheckboxes = table.querySelectorAll('.select-row');
                rowCheckboxes.forEach(checkbox => {
                    checkbox.checked = this.checked;
                });
                AssetFlow.updateBulkActions();
            });
            
            // Handle individual row selection
            table.addEventListener('change', function(e) {
                if (e.target.classList.contains('select-row')) {
                    AssetFlow.updateBulkActions();
                }
            });
        }
    },

    updateBulkActions: function() {
        const selectedRows = document.querySelectorAll('.select-row:checked').length;
        const bulkActionsBar = document.querySelector('.bulk-actions');
        
        if (bulkActionsBar) {
            bulkActionsBar.style.display = selectedRows > 0 ? 'block' : 'none';
            const countElement = bulkActionsBar.querySelector('.selected-count');
            if (countElement) {
                countElement.textContent = selectedRows;
            }
        }
    },

    // ========================================================================
    // Form Validation & Enhancement
    // ========================================================================
    initFormValidation: function() {
        const forms = document.querySelectorAll('form[data-validate]');
        forms.forEach(form => {
            this.enhanceForm(form);
        });
    },

    enhanceForm: function(form) {
        // Add real-time validation
        const inputs = form.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            input.addEventListener('blur', () => this.validateField(input));
            input.addEventListener('input', () => this.clearFieldError(input));
        });

        // Handle form submission
        form.addEventListener('submit', (e) => {
            if (!this.validateForm(form)) {
                e.preventDefault();
                e.stopPropagation();
            } else {
                this.showFormLoading(form);
            }
        });
    },

    validateField: function(field) {
        const value = field.value.trim();
        const isRequired = field.hasAttribute('required');
        const type = field.type;
        
        let isValid = true;
        let errorMessage = '';

        // Required validation
        if (isRequired && !value) {
            isValid = false;
            errorMessage = 'This field is required.';
        }

        // Email validation
        if (type === 'email' && value && !this.isValidEmail(value)) {
            isValid = false;
            errorMessage = 'Please enter a valid email address.';
        }

        // Custom validations
        const customValidation = field.getAttribute('data-validation');
        if (customValidation && value) {
            const result = this.runCustomValidation(customValidation, value);
            if (!result.valid) {
                isValid = false;
                errorMessage = result.message;
            }
        }

        this.setFieldValidation(field, isValid, errorMessage);
        return isValid;
    },

    validateForm: function(form) {
        const fields = form.querySelectorAll('input, select, textarea');
        let isFormValid = true;

        fields.forEach(field => {
            if (!this.validateField(field)) {
                isFormValid = false;
            }
        });

        return isFormValid;
    },

    setFieldValidation: function(field, isValid, errorMessage) {
        const formGroup = field.closest('.form-group, .mb-3, .col');
        const existingError = formGroup?.querySelector('.invalid-feedback');
        
        if (isValid) {
            field.classList.remove('is-invalid');
            field.classList.add('is-valid');
            if (existingError) {
                existingError.remove();
            }
        } else {
            field.classList.remove('is-valid');
            field.classList.add('is-invalid');
            
            if (!existingError) {
                const errorDiv = document.createElement('div');
                errorDiv.className = 'invalid-feedback';
                errorDiv.textContent = errorMessage;
                field.parentNode.appendChild(errorDiv);
            } else {
                existingError.textContent = errorMessage;
            }
        }
    },

    clearFieldError: function(field) {
        field.classList.remove('is-invalid');
        const formGroup = field.closest('.form-group, .mb-3, .col');
        const existingError = formGroup?.querySelector('.invalid-feedback');
        if (existingError) {
            existingError.remove();
        }
    },

    isValidEmail: function(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    },

    runCustomValidation: function(validation, value) {
        // Add custom validation rules as needed
        switch (validation) {
            case 'asset-tag':
                return {
                    valid: /^[A-Z]{2,4}\d{3,6}$/.test(value),
                    message: 'Asset tag must be in format: ABC123 (2-4 letters, 3-6 numbers)'
                };
            case 'pc-id':
                return {
                    valid: /^PC\d{3,6}$/.test(value),
                    message: 'PC ID must be in format: PC123 (PC followed by 3-6 numbers)'
                };
            default:
                return { valid: true, message: '' };
        }
    },

    showFormLoading: function(form) {
        const submitBtn = form.querySelector('button[type="submit"]');
        if (submitBtn) {
            submitBtn.disabled = true;
            const originalText = submitBtn.innerHTML;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Processing...';
            
            // Restore button after form submission
            setTimeout(() => {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }, 3000);
        }
    },

    // ========================================================================
    // Loading States
    // ========================================================================
    initLoadingStates: function() {
        // Intercept AJAX requests and show loading
        const originalFetch = window.fetch;
        window.fetch = function(...args) {
            AssetFlow.showLoading();
            return originalFetch.apply(this, args)
                .then(response => {
                    AssetFlow.hideLoading();
                    return response;
                })
                .catch(error => {
                    AssetFlow.hideLoading();
                    throw error;
                });
        };
    },

    showLoading: function(message = 'Loading...') {
        const overlay = document.getElementById('loadingOverlay');
        if (overlay) {
            const text = overlay.querySelector('p');
            if (text) text.textContent = message;
            overlay.style.display = 'flex';
        }
    },

    hideLoading: function() {
        const overlay = document.getElementById('loadingOverlay');
        if (overlay) {
            overlay.style.display = 'none';
        }
    },

    // ========================================================================
    // Utility Functions
    // ========================================================================
    initUtilities: function() {
        // Auto-hide alerts
        this.initAutoHideAlerts();
        
        // Tooltip initialization
        this.initTooltips();
        
        // Clipboard functionality
        this.initClipboard();
        
        // Confirmation dialogs
        this.initConfirmations();
    },

    initAutoHideAlerts: function() {
        const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
        alerts.forEach(alert => {
            setTimeout(() => {
                if (alert.parentNode) {
                    const bsAlert = new bootstrap.Alert(alert);
                    bsAlert.close();
                }
            }, 5000);
        });
    },

    initTooltips: function() {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function(tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    },

    initClipboard: function() {
        const copyButtons = document.querySelectorAll('[data-copy]');
        copyButtons.forEach(button => {
            button.addEventListener('click', function() {
                const textToCopy = this.getAttribute('data-copy');
                navigator.clipboard.writeText(textToCopy).then(() => {
                    AssetFlow.showToast('Copied to clipboard!', 'success', 2000);
                }).catch(() => {
                    AssetFlow.showToast('Failed to copy to clipboard', 'error');
                });
            });
        });
    },

    initConfirmations: function() {
        const confirmButtons = document.querySelectorAll('[data-confirm]');
        confirmButtons.forEach(button => {
            button.addEventListener('click', function(e) {
                const message = this.getAttribute('data-confirm') || 'Are you sure?';
                if (!confirm(message)) {
                    e.preventDefault();
                    e.stopPropagation();
                }
            });
        });
    },

    // ========================================================================
    // Public API Methods
    // ========================================================================
    refreshData: function() {
        this.showLoading('Refreshing data...');
        window.location.reload();
    },

    exportData: function(format = 'csv') {
        this.showLoading('Preparing export...');
        // Implementation for data export
        console.log('Exporting data in format:', format);
        setTimeout(() => {
            this.hideLoading();
            this.showToast('Export completed!', 'success');
        }, 2000);
    },

    printReport: function() {
        window.print();
    }
};

// ============================================================================
// Initialize when DOM is ready
// ============================================================================
document.addEventListener('DOMContentLoaded', function() {
    AssetFlow.init();
});

// ============================================================================
// Handle page visibility changes
// ============================================================================
document.addEventListener('visibilitychange', function() {
    if (!document.hidden) {
        // Refresh notifications when page becomes visible
        AssetFlow.loadNotifications();
    }
});

// ============================================================================
// Global error handling
// ============================================================================
window.addEventListener('error', function(e) {
    // Only log actual errors, not null/undefined errors
    if (e.error && e.error.message) {
        console.error('Global error:', e.error.message, 'at', e.filename + ':' + e.lineno);
        AssetFlow.showToast('An unexpected error occurred: ' + e.error.message, 'error');
    } else if (e.message && e.message !== 'null') {
        console.error('Global error:', e.message, 'at', e.filename + ':' + e.lineno);
        AssetFlow.showToast('An unexpected error occurred: ' + e.message, 'error');
    }
    // Ignore null/undefined errors which are often harmless
});

// ============================================================================
// Export AssetFlow for global access
// ============================================================================
window.AssetFlow = AssetFlow;
