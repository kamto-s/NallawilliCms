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
        var data = readPageData('cmssection-index-page-data');

        if (data.flashSuccess && window.AdminSwal && AdminSwal.available()) {
            AdminSwal.success(data.flashSuccess);
        }
        if (data.flashError && window.AdminSwal && AdminSwal.available()) {
            AdminSwal.error(data.flashError);
        }

        $(document).on('click', '.btn-cmssection-delete', function () {
            var $btn = $(this);
            var id = $btn.data('id');
            var $card = $btn.closest('.cmssection-card');
            var label =
                ($card.find('.cmssection-card__type').first().text() || '').trim() || '#' + id;
            if (!window.AdminSwal || !AdminSwal.available()) {
                if (confirm('Delete this section?')) {
                    $('#cmssection-delete-id').val(id);
                    $('#cmssection-delete-form').trigger('submit');
                }
                return;
            }
            AdminSwal.confirmDanger({
                title: 'Delete section?',
                html:
                    'Section <strong>' +
                    AdminSwal.escapeHtml(label) +
                    '</strong> will be archived (soft delete).',
                confirmButtonText: 'Yes, delete',
                cancelButtonText: 'Cancel'
            }).then(function (result) {
                if (result.isConfirmed) {
                    $('#cmssection-delete-id').val(id);
                    $('#cmssection-delete-form').trigger('submit');
                }
            });
        });
    });
})();
