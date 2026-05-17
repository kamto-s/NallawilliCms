/**
 * Admin SweetAlert2 helpers — one place for colors, titles, and confirm flows.
 * Depends on Swal (sweetalert2.all) loaded before this file.
 */
(function (global) {
    'use strict';
    var C = {
        primary: '#3D7FF9',
        danger: '#dc3545',
        muted: '#6c757d'
    };
    function available() {
        return typeof global.Swal !== 'undefined' && global.Swal && typeof global.Swal.fire === 'function';
    }
    function fire(opts) {
        if (!available()) {
            return Promise.reject(new Error('SweetAlert2 is not loaded'));
        }
        return global.Swal.fire(opts);
    }

    /**
     * Escape text for safe insertion inside Swal html (e.g. user-provided names).
     */
    function escapeHtml(value) {
        if (value == null) return '';
        var div = document.createElement('div');
        div.textContent = String(value);
        return div.innerHTML;
    }
    function success(text, title) {
        return fire({
            icon: 'success',
            title: title || 'Success',
            text: text || undefined,
            confirmButtonColor: C.primary,
            showCancelButton: false
        });
    }
    function error(text, title) {
        return fire({
            icon: 'error',
            title: title || 'Something went wrong',
            text: text || undefined,
            confirmButtonText: 'OK',
            confirmButtonColor: C.danger,
            showCancelButton: false
        });
    }

    /** Single-button warning (no cancel). */
    function warning(text, title) {
        return fire({
            icon: 'warning',
            title: title || '',
            text: text || undefined,
            confirmButtonText: 'OK',
            confirmButtonColor: C.primary,
            showCancelButton: false
        });
    }

    /**
     * Two-step confirm dialog (destructive actions). Merges your Swal options with danger-styled defaults.
     * @param {object} options Swal.fire options (title, html, text, icon, confirmButtonText, cancelButtonText, …)
     */
    function confirmDanger(options) {
        options = options || {};
        var merged = Object.assign(
            {
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Confirm',
                cancelButtonText: 'Cancel',
                confirmButtonColor: C.danger,
                cancelButtonColor: C.muted
            },
            options
        );
        if (!available()) {
            var msg = merged.text || merged.title || 'Confirm?';
            return Promise.resolve({ isConfirmed: global.confirm(msg) });
        }
        return global.Swal.fire(merged);
    }

    global.AdminSwal = {
        colors: C,
        available: available,
        escapeHtml: escapeHtml,
        success: success,
        error: error,
        warning: warning,
        confirmDanger: confirmDanger
    };
})(window);
