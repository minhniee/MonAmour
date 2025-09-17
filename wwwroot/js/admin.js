// Admin Dashboard JavaScript

document.addEventListener('DOMContentLoaded', function() {
    // Toggle sidebar functionality
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.querySelector('.sidebar');
    const mainContent = document.querySelector('.main-content');
    
    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function() {
            const adminDashboard = document.querySelector('.admin-dashboard');
            
            if (window.innerWidth <= 768) {
                // Mobile: toggle between collapsed and expanded
                sidebar.classList.toggle('collapsed');
                if (adminDashboard) {
                    adminDashboard.classList.toggle('collapsed');
                }
                
                // Store preference in localStorage
                const isCollapsed = sidebar.classList.contains('collapsed');
                localStorage.setItem('sidebarCollapsed', isCollapsed);
            } else {
                // Desktop: collapse/expand sidebar
                sidebar.classList.toggle('collapsed');
                if (adminDashboard) {
                    adminDashboard.classList.toggle('collapsed');
                }
                
                // Store preference in localStorage
                const isCollapsed = sidebar.classList.contains('collapsed');
                localStorage.setItem('sidebarCollapsed', isCollapsed);
            }
        });
    }
    
    // Restore sidebar state from localStorage on page load
    if (sidebar) {
        const adminDashboard = document.querySelector('.admin-dashboard');
        const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        if (isCollapsed) {
            sidebar.classList.add('collapsed');
            if (adminDashboard) {
                adminDashboard.classList.add('collapsed');
            }
        }
    }

    // Note: Sidebar is now always visible on both desktop and mobile

    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });

    // Search functionality
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                // Implement search functionality here
                console.log('Searching for:', this.value);
            }
        });
    }

    // User profile dropdown
    const userProfile = document.querySelector('.user-profile');
    if (userProfile) {
        userProfile.addEventListener('click', function() {
            // Implement user profile dropdown here
            console.log('User profile clicked');
        });
    }

    // Floating settings button
    const floatingSettings = document.querySelector('.floating-settings');
    if (floatingSettings) {
        floatingSettings.addEventListener('click', function() {
            // Implement settings modal here
            console.log('Settings clicked');
        });
    }

    // Notification badges
    const notificationItems = document.querySelectorAll('.nav-item');
    notificationItems.forEach(function(item) {
        item.addEventListener('click', function() {
            const badge = this.querySelector('.badge');
            if (badge) {
                // Implement notification functionality here
                console.log('Notification clicked');
            }
        });
    });

    // Responsive sidebar behavior
    function handleResize() {
        const adminDashboard = document.querySelector('.admin-dashboard');
        // Both desktop and mobile now use the same collapsed behavior
        const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        if (isCollapsed) {
            sidebar.classList.add('collapsed');
            if (adminDashboard) {
                adminDashboard.classList.add('collapsed');
            }
        } else {
            sidebar.classList.remove('collapsed');
            if (adminDashboard) {
                adminDashboard.classList.remove('collapsed');
            }
        }
    }

    window.addEventListener('resize', handleResize);

    // Initialize charts if Chart.js is loaded
    if (typeof Chart !== 'undefined') {
        initializeCharts();
    }
});

// Chart initialization function
function initializeCharts() {
    // User Statistics Chart
    const userStatsCtx = document.getElementById('userStatsChart');
    if (userStatsCtx) {
        new Chart(userStatsCtx.getContext('2d'), {
            type: 'line',
            data: {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
                datasets: [{
                    label: 'Subscribers',
                    data: [65, 59, 80, 81, 56, 55, 40, 45, 60, 70, 85, 90],
                    borderColor: '#dc3545',
                    backgroundColor: 'rgba(220, 53, 69, 0.1)',
                    fill: true
                }, {
                    label: 'New Visitors',
                    data: [28, 48, 40, 19, 86, 27, 90, 85, 70, 80, 75, 85],
                    borderColor: '#fd7e14',
                    backgroundColor: 'rgba(253, 126, 20, 0.1)',
                    fill: true
                }, {
                    label: 'Active Users',
                    data: [90, 85, 95, 88, 92, 89, 85, 90, 95, 98, 92, 95],
                    borderColor: '#007bff',
                    backgroundColor: 'rgba(0, 123, 255, 0.1)',
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 100
                    }
                }
            }
        });
    }

    // Sales Chart
    const salesCtx = document.getElementById('salesChart');
    if (salesCtx) {
        new Chart(salesCtx.getContext('2d'), {
            type: 'line',
            data: {
                labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
                datasets: [{
                    label: 'Sales',
                    data: [650, 590, 800, 810, 560, 550, 400],
                    borderColor: 'rgba(255, 255, 255, 0.8)',
                    backgroundColor: 'rgba(255, 255, 255, 0.1)',
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    x: {
                        display: false
                    },
                    y: {
                        display: false
                    }
                },
                elements: {
                    point: {
                        radius: 0
                    }
                }
            }
        });
    }

    // Online Users Chart
    const onlineCtx = document.getElementById('onlineChart');
    if (onlineCtx) {
        new Chart(onlineCtx.getContext('2d'), {
            type: 'line',
            data: {
                labels: ['', '', '', '', '', '', ''],
                datasets: [{
                    label: 'Online Users',
                    data: [12, 15, 18, 14, 16, 17, 17],
                    borderColor: '#007bff',
                    backgroundColor: 'rgba(0, 123, 255, 0.1)',
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    x: {
                        display: false
                    },
                    y: {
                        display: false
                    }
                },
                elements: {
                    point: {
                        radius: 0
                    }
                }
            }
        });
    }
}

// Utility functions
function showNotification(message, type = 'info') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    const container = document.querySelector('.page-content');
    if (container) {
        container.insertBefore(alertDiv, container.firstChild);
        
        // Auto-hide after 5 seconds
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alertDiv);
            bsAlert.close();
        }, 5000);
    }
}

function updateBadgeCount(elementId, count) {
    const badge = document.querySelector(`#${elementId} .badge`);
    if (badge) {
        badge.textContent = count;
        if (count > 0) {
            badge.style.display = 'flex';
        } else {
            badge.style.display = 'none';
        }
    }
}

// Export functions for use in other scripts
window.AdminDashboard = {
    showNotification,
    updateBadgeCount,
    initializeCharts
};
