

(function () {

    const Editor = toastui.Editor;
    const codeSyntaxHighlight = Editor.plugin.codeSyntaxHighlight;


    const input = document.getElementById("mdeText");

    const editor = new Editor({
        el: document.querySelector('#mde'),
        usageStatistics: false,
        height: 'auto',
        previewStyle: 'tab',
        language: 'de-DE',
        theme: 'dark',
        autofocus: false,
        initialValue: input.value,
        toolbarItems: [
            ['heading', 'bold', 'italic', 'strike'],
            ['code', 'codeblock'],
            ['ul', 'ol'],
            ['hr', 'quote'],
        ],
        events: {
            change: () => {
                input.value = editor.getMarkdown()
                if (window.autosave) {
                    window.autosave(input.value);
                }
            }
        },
        plugins: [codeSyntaxHighlight]
    });


    window.updateMde = () => editor.setMarkdown(input.value, true);

    window.editor = editor;


    $('#tags').tagsinput({
        confirmKeys: [13, 44, 32, 188],
        trimValue: true,
        tagClass: 'badge bg-primary'
    });
})();