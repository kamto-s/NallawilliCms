(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        if (typeof Quill === 'undefined') return;
        var ta = document.getElementById('Content');
        var host = document.getElementById('post-quill-editor');
        var form = document.getElementById('post-form');
        if (!ta || !host || !form) return;

        var quill = new Quill('#post-quill-editor', {
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
            ta.value = quill.root.innerHTML;
        }

        quill.on('text-change', function () {
            ta.value = quill.root.innerHTML;
        });

        form.addEventListener('submit', function (e) {
            ta.value = quill.root.innerHTML;
            if (!quill.getText().trim()) {
                e.preventDefault();
                if (window.AdminSwal && AdminSwal.available()) {
                    AdminSwal.warning('Please add some content to the body.', 'Content required');
                } else {
                    alert('Please add some content to the body.');
                }
            }
        });
    });
})();
