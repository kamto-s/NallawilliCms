(function () {
    'use strict';

    function readPageData(id) {
        var el = document.getElementById(id);
        if (!el || !el.textContent) return {};
        try {
            return JSON.parse(el.textContent);
        } catch (e) {
            return {};
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        var data = readPageData('cmspage-delete-page-data');
        if (data.flashError && window.AdminSwal && AdminSwal.available()) {
            AdminSwal.error(data.flashError, 'Cannot delete');
        }

        var btn = document.getElementById('btn-delete-cmspage-confirm');
        if (!btn) return;
        btn.addEventListener('click', function () {
            var pageTitle = data.pageTitle || '';
            var form = document.getElementById('cmspage-delete-confirm-form');
            if (!form) return;

            if (!window.AdminSwal || !AdminSwal.available()) {
                if (confirm('Delete this page?')) form.submit();
                return;
            }

            AdminSwal.confirmDanger({
                title: 'Delete this page?',
                html: 'Page <strong>' + AdminSwal.escapeHtml(pageTitle) + '</strong> will be archived (soft delete), including its sections.',
                confirmButtonText: 'Yes, delete',
                cancelButtonText: 'Cancel'
            }).then(function (r) {
                if (r.isConfirmed) form.submit();
            });
        });
    });
})();
