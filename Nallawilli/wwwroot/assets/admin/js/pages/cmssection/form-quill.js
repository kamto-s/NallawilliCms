(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        if (typeof Quill === 'undefined') return;

        var ta = document.getElementById('Description');
        var host = document.getElementById('cmssection-quill-description');
        var form = document.getElementById('cmssection-form');
        if (!ta || !host || !form) return;

        var quill = new Quill('#cmssection-quill-description', {
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
