window.quillInterop = {
    attachChangeHandler: function (editorContainerId, dotNetHelper) {
        const container = document.getElementById(editorContainerId);
        if (!container) {
            console.warn('Editor container not found: ' + editorContainerId);
            return;
        }

        // Find Quill instance
        let quill = null;
        for (const child of container.children) {
            if (child.__quill) {
                quill = child.__quill;
                break;
            }
        }

        if (!quill) {
            console.warn('Quill instance not found in container: ' + editorContainerId);
            return;
        }

        quill.on('text-change', function () {
            dotNetHelper.invokeMethodAsync('NotifyContentChanged');
        });
    }
};
