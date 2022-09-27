(function () {
    let currentTheme;
    let savedTheme;

    let cssElement = document.getElementById("css");

    function getCookie(name) {
        var v = document.cookie.match('(^|;) ?' + name + '=([^;]*)(;|$)');
        return v ? v[2] : null;
    }
    function setCookie(name, value, days) {
        var d = new Date;
        d.setTime(d.getTime() + 24 * 60 * 60 * 1000 * days);
        document.cookie = name + "=" + value + ";path=/;expires=" + d.toGMTString();
    }


    let cookie = getCookie("theme");

    if (!cookie) {
        savedTheme = "auto";
    } else {
        savedTheme = cookie == "dark" ? "dark" : cookie == "light" ? "light" : "auto";
    }

    function updateCurrentTheme() {
        if (savedTheme == "auto")
            currentTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? "dark" : "light";
        else
            currentTheme = savedTheme;

        window.darkMode = currentTheme == "dark";
    }
    updateCurrentTheme();

    function updateButton() {
        let icon = currentTheme == "dark" ? "fa-sun" : "fa-moon";

        let icons = $(".darkModeIcon");
        icons.removeClass();
        icons.addClass("darkModeIcon");
        icons.addClass("fas");
        icons.addClass(icon);
    }
    updateButton();

    function setTheme(theme) {
        cssElement.href = "/css/theme." + theme + ".css"
        setCookie("theme", theme, 10000);
        savedTheme = theme;
        updateCurrentTheme();
        updateButton();
    }

    function toggleTheme() {
        let newTheme = currentTheme == "dark" ? "light" : "dark";
        setTheme(newTheme);
    }

    $(".darkModeButton").click(toggleTheme);

    function enableDarkMode() {
        document.cookie = "theme=dark; path=/;";

        document.children[0].dataset.theme = "dark";

        newTheme = "/lib/bootstrap-dark/dist/bootstrap-night.min.css";
        document.getElementById("css").href = newTheme;
    }

    function disableDarkMode() {
        document.cookie = "theme=light; path=/;";
        document.children[0].dataset.theme = "light";
        newTheme = "/lib/bootstrap/css/bootstrap.min.css";
        document.getElementById("css").href = newTheme;
    }

    function toggleDarkMode() {
        let cookie = document.cookie
            .split('; ')
            .find(row => row.startsWith('theme='));


        if (cookie && cookie.endsWith("=dark")) {
            disableDarkMode();
        } else {
            enableDarkMode();
        }
    }
})();