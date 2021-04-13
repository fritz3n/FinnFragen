

(function () {
    var simplemde = new SimpleMDE({
        forceSync: true,
        spellChecker: false,
        element: document.getElementById("mde"),
        status: false,
        placeholder: "## Deine Frage",
        autofocus: true,
        showIcons: [
            "bold",
            "italic",
            "strikethrough",
            "heading",
            "|",
            "code",
            "quote",
            "unordered-list",
            "ordered-list",
            "|",
            "link",
            "image",
            "table",
            "|",
            "preview",
            "side-by-side",
            "fullscreen",
            "|",
            "guide"
        ]
    });
    $('#tags').tagsinput({
        confirmKeys: [13, 44, 32, 188],
        trimValue: true
    });
})();