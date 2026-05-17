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

    jQuery(function ($) {
        var data = readPageData('post-index-page-data');
        var tableSelector = '#postsTable';
        if ($(tableSelector).length && !($.isDataTable && $.isDataTable(tableSelector))) {
            $(tableSelector).DataTable({
                order: [[0, 'desc']],
                pageLength: 10,
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'All']],
                language: {
                    lengthMenu: '_MENU_\u00a0entries per page'
                },
                autoWidth: false,
                columnDefs: [
                    { targets: 1, orderable: false, searchable: false },
                    { targets: 6, orderable: false, searchable: false }
                ]
            });
        }

        if (data.flashSuccess && window.AdminSwal && AdminSwal.available()) {
            AdminSwal.success(data.flashSuccess);
        }
        if (data.flashError && window.AdminSwal && AdminSwal.available()) {
            AdminSwal.error(data.flashError);
        }

        // Delete buttons live inside .dropdown-menu; main.js stops propagation there,
        // so document-level delegation never runs — bind on the menu instead.
        $('.dropdown-menu').on('click', '.btn-post-delete', function () {
            var id = $(this).data('id');
            var title = $(this).closest('tr').find('td').eq(1).text().trim() || ('#' + id);
            if (!window.AdminSwal || !AdminSwal.available()) {
                if (confirm('Delete this post?')) {
                    $('#post-delete-id').val(id);
                    $('#post-delete-form').trigger('submit');
                }
                return;
            }
            AdminSwal.confirmDanger({
                title: 'Delete post?',
                html: 'Post <strong>' + AdminSwal.escapeHtml(title) + '</strong> will be archived (soft delete).',
                confirmButtonText: 'Yes, delete',
                cancelButtonText: 'Cancel'
            }).then(function (result) {
                if (result.isConfirmed) {
                    $('#post-delete-id').val(id);
                    $('#post-delete-form').trigger('submit');
                }
            });
        });
    });
})();
