(() => {
    function nullOrEmpty(string) {
        return string == null || string.trim() == "";
    }

    const key = "markdown"
    var active = false;

    window.autosave = (markdown) => {
        if (active) {
            localStorage.setItem(key, markdown);
        }
    }

    window.clearAutosave = () => {
        localStorage.removeItem(key);
    }

    const input = document.getElementById("mdeText");

    if (input != null) {

        restore = () => {
            let storage = localStorage.getItem(key);
            if (!nullOrEmpty(storage))
                input.value = storage;
        }

        canRestore = () => {
            let storage = localStorage.getItem(key);
            return !nullOrEmpty(storage);
        }

        const restoreButton = document.getElementById("restoreButton");
        const restoreButtonSection = document.getElementById("restoreButtonSection");
        restoreButton.onclick = () => {
            restore();
            window.updateMde();
            restoreButtonSection.classList.remove("show");
            active = true;
        }

        if (nullOrEmpty(input.value)) {
            if (canRestore())
                restore();
            active = true;
        } else if (canRestore) {
            restoreButtonSection.classList.add("show");
            setTimeout(() => {
                active = true;
                restoreButtonSection.classList.remove("show");
            }, 1000 * 10)
        } else {
            active = true;
        }
    }

})();