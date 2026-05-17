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
        var data = readPageData('category-delete-page-data');
        if (data.flashError && window.AdminSwal && AdminSwal.available()) {
            AdminSwal.error(data.flashError, 'Cannot delete');
        }
        if (!data.flashError && data.hasPosts && window.AdminSwal && AdminSwal.available()) {
            AdminSwal.warning(
                'Remove or reassign posts that use this category before deleting it.',
                'Posts still linked'
            );
        }

        var btn = document.getElementById('btn-delete-category-confirm');
        if (!btn) return;
        btn.addEventListener('click', function () {
            var categoryName = data.categoryName || '';
            var form = document.getElementById('category-delete-confirm-form');
            if (!form) return;

            if (!window.AdminSwal || !AdminSwal.available()) {
                if (confirm('Delete this category?')) form.submit();
                return;
            }

            AdminSwal.confirmDanger({
                title: 'Delete this category?',
                html: 'Category <strong>' + AdminSwal.escapeHtml(categoryName) + '</strong> will be archived (soft delete).',
                confirmButtonText: 'Yes, delete',
                cancelButtonText: 'Cancel'
            }).then(function (r) {
                if (r.isConfirmed) form.submit();
            });
        });
    });
})();
