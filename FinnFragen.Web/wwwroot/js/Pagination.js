let onChange;
let getPageination;
let updateTotalCount = () => { };

let testPagination;

(function () {
    let totalCount = -1;
    let totalPageCount = -1;
    let currentPage = 0;
    let resultCount = 10;

    let doUpdate = false;

    let pagination = document.getElementById("pagination");

    function setHtml() {
        function newLI(content, ...classes) {
            let li = document.createElement("li");
            li.classList.add("page-item", ...classes);
            li.appendChild(content);
            return li;
        }

        function newButtonLI(text, clickHandler = null, page = null, buttonClass = null, breakpoint = null) {
            let button = document.createElement("button");
            button.innerHTML = text; // must be innerHTML
            button.classList.add("page-link");
            if (clickHandler != null)
                button.onclick = clickHandler;

            if (page != null)
                button.dataset.page = page;

            let classes = [];
            if (buttonClass != null)
                classes.push(buttonClass);

            if (breakpoint != null)
                classes.push("d-none", "d-" + breakpoint + "-block");

            return newLI(button, ...classes);
        }

        pagination.innerHTML = "";



        let frontgap = currentPage;
        let backgap = totalPageCount - currentPage -1;

        let frontspacer = true;
        let backspacer = true;

        let frontdisplay;
        let backdisplay;

        if (frontgap < 4 && backgap < 4) {
            frontspacer = backspacer = false;
            frontdisplay = frontgap;
            backdisplay = backgap;
        }else if (frontgap < 4) {
            frontspacer = false;
            frontdisplay = frontgap;
            backdisplay = 8 - frontgap;
        } else if (backgap < 4) {
            backspacer = false;
            frontdisplay = 8 - backgap;
            backdisplay = backgap;
        } else {
            frontdisplay = backdisplay = 4;
            frontspacer = frontgap > 5;
            backspacer = backgap > 5;
        }

        pagination.appendChild(newButtonLI("&laquo;", clickPrev, null, currentPage == 0 ? "disabled" : null));
        if (frontgap > 4)
            pagination.appendChild(newButtonLI("1", clickPage, 0));
        if (frontspacer) {
            pagination.appendChild(newButtonLI("...", null, null, "disabled"));
        }


        for (let i = currentPage - frontdisplay; i <= currentPage + backdisplay; i++) {
            let distance = Math.abs(currentPage - i);
            let breakpoint;

            if (distance < 2)
                breakpoint = null;
            else if (distance < Math.max(backdisplay, frontdisplay) / 2)
                breakpoint = "sm";
            else
                breakpoint = "md";

            pagination.appendChild(newButtonLI("" + (i + 1), clickPage, i, distance == 0 ? "active" : null, breakpoint));
        }

        if (backspacer)
            pagination.appendChild(newButtonLI("...", null, null, "disabled"));
        if (backgap > 4)
            pagination.appendChild(newButtonLI("" + (totalPageCount), clickPage, totalPageCount - 1));

        pagination.appendChild(newButtonLI("&raquo;", clickNext, null, currentPage == totalPageCount - 1 ? "disabled" : null));

        let counts = [15, 25, 50, 100];

        let selectContainer = document.createElement("div");
        selectContainer.classList.add("form-group", "d-inline");

        let select = document.createElement("select");
        select.classList.add("form-control");

        let option = document.createElement("option");
        option.value = 10;
        option.text = "10 pro Seite";
        select.appendChild(option);

        for (let i = 0; i < counts.length; i++) {
            let count = counts[i];
            if (totalPageCount < count)
                break;

            let option = document.createElement("option");
            option.value = count;
            option.text = count + " pro Seite";
            select.appendChild(option);
        }

        option = document.createElement("option");
        option.value = -1;
        option.text = "Alle auf einer Seite";
        select.appendChild(option);

        select.onchange = changeCount;

        selectContainer.appendChild(select);
        pagination.appendChild(newLI(selectContainer, "ml-2"));
    }


    function clickPrev() {
        if (currentPage != 0)
            currentPage--;
        onChange(true);
    }
    function clickNext() {
        if (currentPage != totalPageCount - 1)
            currentPage--;
        onChange(true);
    }
    function clickPage() {
        let page = this.dataset.page;
        if (!page)
            return;
        if (page >= 0 && page < totalPageCount && page != currentPage) {
            currentPage = page;
            onChange(true);
        }
    }
    function changeCount(obj) {
        resultCount = obj.value == -1 ? Infinity : obj.value;
        doUpdate = true;
        onChange(true);
    }

    getPageination = function () {
        if (!resultCount || !isFinite(resultCount))
            return null;

        return {}  //"from=" + (currentPage * resultCount) + "&take=" + resultCount;

    };

    updateTotalCount = function (n) {
        let prev = totalCount;
        totalCount = n;
        totalPageCount = Math.ceil(totalCount / resultCount);

        if (prev != totalCount || doUpdate) {
            doUpdate = false;
            setHtml();
        }
    };

    testPagination = function (current = null, total = null) {
        if (current != null)
            currentPage = current;
        if (total != null)
            totalPageCount = total;
        setHtml();
    }

})();
