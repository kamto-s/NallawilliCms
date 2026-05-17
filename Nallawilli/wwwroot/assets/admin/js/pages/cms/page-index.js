(function () {
    'use strict';
    jQuery(function () {
        var data = CmsAdminList.readPageData('cms-page-index-data');
        var orderCol = $('#cmsPagesTable thead th').length - 1;
        CmsAdminList.initDataTable('#cmsPagesTable', Math.max(0, orderCol - 1));
        CmsAdminList.showFlash(data);
        if (document.getElementById('cms-delete-form')) {
            CmsAdminList.bindDeleteButtons('page');
        }
    });
})();
