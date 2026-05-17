(function () {
    'use strict';
    jQuery(function () {
        var data = CmsAdminList.readPageData('cms-content-index-data');
        CmsAdminList.initDataTable('#cmsContentsTable', 3);
        CmsAdminList.showFlash(data);
        CmsAdminList.bindDeleteButtons('content item');
    });
})();
