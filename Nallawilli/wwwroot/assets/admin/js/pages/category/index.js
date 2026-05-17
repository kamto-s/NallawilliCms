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
        var data = readPageData('category-index-page-data');
        var tableSelector = '#categoriesTable';
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
                    { targets: 1, orderable: false },
                    { targets: 3, orderable: false, searchable: false }
                ]
            });
        }

        if (data.flashSuccess && window.AdminSwal && AdminSwal.available()) {
            AdminSwal.success(data.flashSuccess);
        }
        if (data.flashError && window.AdminSwal && AdminSwal.available()) {
            AdminSwal.error(data.flashError);
        }

        $(document).on('click', '.btn-category-delete', function () {
            var id = $(this).data('id');
            var name = $(this).closest('tr').find('td').eq(1).text().trim() || ('#' + id);
            if (!window.AdminSwal || !AdminSwal.available()) {
                if (confirm('Delete this category?')) {
                    $('#category-delete-id').val(id);
                    $('#category-delete-form').trigger('submit');
                }
                return;
            }
            AdminSwal.confirmDanger({
                title: 'Delete category?',
                html: 'Category <strong>' + AdminSwal.escapeHtml(name) + '</strong> will be archived (soft delete).',
                confirmButtonText: 'Yes, delete',
                cancelButtonText: 'Cancel'
            }).then(function (result) {
                if (result.isConfirmed) {
                    $('#category-delete-id').val(id);
                    $('#category-delete-form').trigger('submit');
                }
            });
        });
    });
})();
