(function () {
    'use strict';

    function hintClass(statusVal) {
        var base = 'rounded-8 p-12 text-13 mb-0 border ';
        if (statusVal === 2) return base + 'border-success-200 bg-success-50 text-success-700';
        if (statusVal === 1) return base + 'border-warning-200 bg-warning-50 text-gray-800';
        return base + 'border-gray-200 bg-gray-50 text-gray-700';
    }

    function syncPostStatusUi() {
        var sel = document.getElementById('post-status-select');
        var group = document.getElementById('post-schedule-group');
        var hint = document.getElementById('post-status-hint');
        var meta = document.getElementById('post-form-meta');
        if (!sel || !group) return;

        var raw = sel.value;
        var v = parseInt(raw, 10);
        if (isNaN(v) && raw === 'Scheduled') v = 1;
        else if (isNaN(v) && raw === 'Published') v = 2;
        else if (isNaN(v) && raw === 'Draft') v = 0;
        else if (isNaN(v)) v = 0;

        if (v === 1) group.classList.remove('d-none');
        else group.classList.add('d-none');

        if (hint && meta) {
            var key = v === 2 ? 'published' : (v === 1 ? 'scheduled' : 'draft');
            var text = meta.getAttribute('data-hint-' + key) || '';
            hint.textContent = text;
            hint.className = hintClass(v);
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        var sel = document.getElementById('post-status-select');
        if (sel) sel.addEventListener('change', syncPostStatusUi);
        syncPostStatusUi();
    });
})();
