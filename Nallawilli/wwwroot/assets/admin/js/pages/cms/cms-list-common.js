(function (global) {
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

    function initDataTable(selector, orderCol) {
        if (!global.jQuery) return;
        var $ = global.jQuery;
        if (!$(selector).length) return;
        if ($.isDataTable && $.isDataTable(selector)) return;
        $(selector).DataTable({
            order: [[orderCol || 0, 'asc']],
            pageLength: 10,
            lengthMenu: [[10, 25, 50, -1], [10, 25, 50, 'All']],
            language: { lengthMenu: '_MENU_\u00a0entries per page' },
            autoWidth: false,
            columnDefs: [{ targets: -1, orderable: false, searchable: false }]
        });
    }

    function showFlash(data) {
        if (data.flashSuccess && global.AdminSwal && AdminSwal.available()) {
            AdminSwal.success(data.flashSuccess);
        }
        if (data.flashError && global.AdminSwal && AdminSwal.available()) {
            AdminSwal.error(data.flashError);
        }
    }

    function bindDeleteButtons(entityLabel) {
        if (!global.jQuery) return;
        var $ = global.jQuery;
        $(document).on('click', '.btn-cms-delete', function () {
            var id = $(this).data('id');
            var name = $(this).data('name') || ('#' + id);
            var submit = function () {
                $('#cms-delete-id').val(id);
                $('#cms-delete-form').trigger('submit');
            };
            if (!global.AdminSwal || !AdminSwal.available()) {
                if (confirm('Delete this ' + entityLabel + '?')) submit();
                return;
            }
            AdminSwal.confirmDanger({
                title: 'Delete ' + entityLabel + '?',
                html: '<strong>' + AdminSwal.escapeHtml(String(name)) + '</strong> will be archived.',
                confirmButtonText: 'Yes, delete',
                cancelButtonText: 'Cancel'
            }).then(function (result) {
                if (result.isConfirmed) submit();
            });
        });
    }

    global.CmsAdminList = {
        readPageData: readPageData,
        initDataTable: initDataTable,
        showFlash: showFlash,
        bindDeleteButtons: bindDeleteButtons
    };
})(window);
