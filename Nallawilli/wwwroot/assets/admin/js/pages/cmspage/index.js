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
        var data = readPageData('cmspage-index-page-data');

        if (data.flashSuccess && window.AdminSwal && AdminSwal.available()) {
            AdminSwal.success(data.flashSuccess);
        }
        if (data.flashError && window.AdminSwal && AdminSwal.available()) {
            AdminSwal.error(data.flashError);
        }

        $(document).on('click', '.btn-cmspage-delete', function () {
            var $btn = $(this);
            var id = $btn.data('id');
            var $card = $btn.closest('.cmspage-card');
            var title =
                ($card.find('.cmspage-card__title').first().text() || '').trim() || '#' + id;
            if (!window.AdminSwal || !AdminSwal.available()) {
                if (confirm('Delete this page?')) {
                    $('#cmspage-delete-id').val(id);
                    $('#cmspage-delete-form').trigger('submit');
                }
                return;
            }
            AdminSwal.confirmDanger({
                title: 'Delete page?',
                html:
                    'Page <strong>' +
                    AdminSwal.escapeHtml(title) +
                    '</strong> will be archived (soft delete), including its sections.',
                confirmButtonText: 'Yes, delete',
                cancelButtonText: 'Cancel'
            }).then(function (result) {
                if (result.isConfirmed) {
                    $('#cmspage-delete-id').val(id);
                    $('#cmspage-delete-form').trigger('submit');
                }
            });
        });
    });
})();
