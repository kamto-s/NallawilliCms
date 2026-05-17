(function () {
    'use strict';
    jQuery(function () {
        var data = CmsAdminList.readPageData('cms-section-index-data');
        CmsAdminList.initDataTable('#cmsSectionsTable', 4);
        CmsAdminList.showFlash(data);
        CmsAdminList.bindDeleteButtons('section');
    });
})();
