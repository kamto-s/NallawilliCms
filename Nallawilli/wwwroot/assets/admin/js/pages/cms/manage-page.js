(function () {
    'use strict';

    var DEFAULT_COLOR = '#55b7a8';

    function readPageData(id) {
        var el = document.getElementById(id);
        if (!el || !el.textContent) return {};
        try {
            return JSON.parse(el.textContent);
        } catch (e) {
            return {};
        }
    }

    function initQuillEditors() {
        if (typeof Quill === 'undefined') return;

        document.querySelectorAll('.cms-quill-host').forEach(function (host) {
            var taId = host.getAttribute('data-textarea-id');
            var ta = taId ? document.getElementById(taId) : null;
            if (!ta || host.__quill) return;

            var quill = new Quill(host, {
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

            host.__quill = quill;
        });
    }

    function syncQuillEditors() {
        document.querySelectorAll('.cms-quill-host').forEach(function (host) {
            if (!host.__quill) return;
            var taId = host.getAttribute('data-textarea-id');
            var ta = taId ? document.getElementById(taId) : null;
            if (ta) ta.value = host.__quill.root.innerHTML;
        });
    }

    function clampRgb(n) {
        var x = parseInt(n, 10);
        if (isNaN(x)) return 0;
        return Math.min(255, Math.max(0, x));
    }

    function componentToHex(n) {
        var x = clampRgb(n);
        return ('0' + x.toString(16)).slice(-2);
    }

    /** Parse #hex, bare hex, rgb()/rgba() → #rrggbb or null */
    function parseColorToHex(value) {
        if (!value) return null;
        var v = value.trim();

        var m6 = /^#?([0-9a-fA-F]{6})$/i.exec(v);
        if (m6) return '#' + m6[1].toLowerCase();

        var m3 = /^#?([0-9a-fA-F]{3})$/i.exec(v);
        if (m3) {
            var h = m3[1];
            return ('#' + h[0] + h[0] + h[1] + h[1] + h[2] + h[2]).toLowerCase();
        }

        var rgb = /^rgba?\(\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})/i.exec(v);
        if (rgb) {
            return '#' + componentToHex(rgb[1]) + componentToHex(rgb[2]) + componentToHex(rgb[3]);
        }

        return null;
    }

    function syncColorField(wrap) {
        var picker = wrap.querySelector('.cms-color-picker');
        var text = wrap.querySelector('.cms-color-text');
        if (!picker || !text) return;

        var editor = wrap.closest('.cms-field-editor');
        var raw = text.value.trim();

        if (!raw) {
            text.value = DEFAULT_COLOR;
            picker.value = DEFAULT_COLOR;
            updateColorSwatch(editor, DEFAULT_COLOR);
            return;
        }

        var hex = parseColorToHex(raw);
        if (hex) {
            text.value = hex;
            picker.value = hex;
            updateColorSwatch(editor, hex);
            return;
        }

        updateColorSwatch(editor, raw);
    }

    function updateColorSwatch(editor, cssColor) {
        var swatch = editor.querySelector('.cms-color-swatch');
        if (swatch && cssColor) swatch.style.backgroundColor = cssColor;
    }

    function bindColorFields() {
        document.querySelectorAll('.cms-color-field').forEach(function (wrap) {
            var picker = wrap.querySelector('.cms-color-picker');
            var text = wrap.querySelector('.cms-color-text');
            var swatchBtn = wrap.querySelector('.cms-color-swatch-btn');
            if (!picker || !text) return;

            var editor = wrap.closest('.cms-field-editor');

            syncColorField(wrap);

            if (swatchBtn) {
                swatchBtn.addEventListener('click', function () {
                    picker.click();
                });
            }

            picker.addEventListener('input', function () {
                text.value = picker.value;
                updateColorSwatch(editor, picker.value);
            });

            text.addEventListener('input', function () {
                var hex = parseColorToHex(text.value);
                if (hex) {
                    picker.value = hex;
                    updateColorSwatch(editor, hex);
                } else if (text.value.trim()) {
                    updateColorSwatch(editor, text.value.trim());
                }
            });

            text.addEventListener('paste', function () {
                setTimeout(function () {
                    syncColorField(wrap);
                }, 0);
            });

            text.addEventListener('change', function () {
                syncColorField(wrap);
            });
        });
    }

    function applyDefaultColorsOnSubmit() {
        document.querySelectorAll('.cms-color-field').forEach(function (wrap) {
            syncColorField(wrap);
        });
    }

    function bindImagePreviews() {
        document.querySelectorAll('.cms-image-file-input').forEach(function (input) {
            input.addEventListener('change', function () {
                var editor = input.closest('.cms-field-editor');
                if (!editor) return;
                var preview = editor.querySelector('.cms-image-preview');
                var file = input.files && input.files[0];

                if (!file) return;

                if (preview) preview.remove();

                preview = document.createElement('div');
                preview.className = 'cms-image-preview mt-12';
                preview.innerHTML =
                    '<img src="" alt="Preview" class="cms-image-preview__img" />' +
                    '<p class="text-gray-500 text-12 mt-8 mb-0">Preview — save the form to upload.</p>';
                input.insertAdjacentElement('afterend', preview);

                var img = preview.querySelector('img');
                img.src = URL.createObjectURL(file);
            });
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        var data = readPageData('cms-manage-page-data');
        if (data.flashSuccess && window.AdminSwal && AdminSwal.available()) {
            AdminSwal.success(data.flashSuccess);
        }

        initQuillEditors();
        bindColorFields();
        bindImagePreviews();

        var form = document.getElementById('cms-manage-form');
        if (form) {
            form.addEventListener('submit', function () {
                syncQuillEditors();
                applyDefaultColorsOnSubmit();
            });
        }
    });
})();
