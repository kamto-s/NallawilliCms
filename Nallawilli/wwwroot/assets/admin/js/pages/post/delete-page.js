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
        var data = readPageData('post-delete-page-data');
        if (data.flashError && window.AdminSwal && AdminSwal.available()) {
            AdminSwal.error(data.flashError, 'Cannot delete');
        }

        var btn = document.getElementById('btn-delete-post-confirm');
        if (!btn) return;
        btn.addEventListener('click', function () {
            var postTitle = data.postTitle || '';
            var form = document.getElementById('post-delete-confirm-form');
            if (!form) return;

            if (!window.AdminSwal || !AdminSwal.available()) {
                if (confirm('Delete this post?')) form.submit();
                return;
            }

            AdminSwal.confirmDanger({
                title: 'Delete this post?',
                html: 'Post <strong>' + AdminSwal.escapeHtml(postTitle) + '</strong> will be archived (soft delete).',
                confirmButtonText: 'Yes, delete',
                cancelButtonText: 'Cancel'
            }).then(function (r) {
                if (r.isConfirmed) form.submit();
            });
        });
    });
})();
