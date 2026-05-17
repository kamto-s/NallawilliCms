(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        if (typeof Quill === 'undefined') return;

        var ta = document.getElementById('MetaDescription');
        var host = document.getElementById('cmspage-quill-meta-description');
        var form = document.getElementById('cmspage-form');
        if (!ta || !host || !form) return;

        var quill = new Quill('#cmspage-quill-meta-description', {
            theme: 'snow',
            modules: {
                toolbar: [
                    [{ header: [1, 2, 3, false] }],
                    ['bold', 'italic', 'underline', 'strike'],
                    [{ list: 'ordered' }, { list: 'bullet' }],
                    ['link'],
                    ['clean']
                ]
            }
        });

        if (ta.value) {
            quill.clipboard.dangerouslyPasteHTML(ta.value);
        }

        quill.on('text-change', function () {
            ta.value = quill.root.innerHTML;
        });

        form.addEventListener('submit', function () {
            ta.value = quill.root.innerHTML;
        });
    });
})();
