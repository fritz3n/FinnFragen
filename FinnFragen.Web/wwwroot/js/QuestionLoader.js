
function escapeHtml(unsafe) {
    return unsafe
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

(function () {
    let list = document.getElementById("list");
    let container = document.getElementById("container");

    async function getData(url) {
        let response = await fetch(url);
        let json = await response.json();
        return json;
    }

    async function getHtml(url) {
        data = await getData(url);
        let html = "";
        data.questions.forEach(q => {
            html += htmlForQuestion(q);
        })
        updateTotalCount(data.totalCount);
        return html;
    }
    let slow = false;

    async function loadHtml(url) {

        let html;
        let isResolved = false;
        let htmlPromise = getHtml(url).then((s) => { html = s; isResolved = true; });
        await Promise.any([delay(slow ? 0 : 300), htmlPromise]);

        if (!isResolved) {
            slow = true;
            container.classList.remove("loaded");
            container.classList.add("loading");
            //list.innerHTML = getPlaceholder();
            await htmlPromise;
        }
        list.innerHTML = html;
        container.classList.add("loaded");
    }


    function delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    let browserUrl = new URL(window.location);

    let tags = document.getElementById("tags");

    $('#tags').tagsinput({
        confirmKeys: [13, 44, 32, 188],
        trimValue: true,
        tagClass: 'badge bg-primary'
    });
    if (browserUrl.searchParams.get("tags"))
        browserUrl.searchParams.get("tags").split(",").forEach(t => $('#tags').tagsinput('add', t))
    $('#tags').on('itemRemoved', () => onChange(true));
    $('#tags').on('itemAdded', () => onChange(true));

    let searchIn = document.getElementById("searchIn");
    if (browserUrl.searchParams.get("search"))
        searchIn.value = browserUrl.searchParams.get("search");
    searchIn.oninput = () => onChange(false);
    let searchForm = document.getElementById("searchForm");
    searchForm.onsubmit = e => { e.preventDefault(); onChange(true) };


    if (browserUrl.searchParams.get("page")) {
        initPageination(
            browserUrl.searchParams.get("page"),
            browserUrl.searchParams.get("count")
        );
    }

    let timer = -1;

    onChange = function(immediate) {
        if (!immediate) {
            clearTimeout(timer);
            timer = setTimeout(() => onChange(true), 1000);
            return;
        }
        clearTimeout(timer);

        let tagString = tags.value;
        let searchString = searchIn.value;

        loadFor(tagString, searchString);
    }

    async function loadFor(tags = null, search = null, setHistory = true) {
        let browserUrl = new URL(window.location);

        let url;
        if (tags) {
            url = "/questions/search?tags=" + encodeURIComponent(tags);
            browserUrl.searchParams.set("tags", tags);
        } else {
            browserUrl.searchParams.delete("tags");
        }

        if (search) {
            if (!url)
                url = "/questions/search?search=" + encodeURIComponent(search);
            else
                url += "&search=" + encodeURIComponent(search);
            browserUrl.searchParams.set("search", search);
        } else {
            browserUrl.searchParams.delete("search");
        }
        if (!url)
            url = "/questions/all";

        url = modifyUrl(url);

        let pagination = getPageination();
        if (pagination != null) {
            if (url.indexOf("?") >= 0) {
                url += "&" + pagination.url;
            } else {
                url += "?" + pagination.url;
            }
            browserUrl.searchParams.set("page", pagination.page);
            browserUrl.searchParams.set("count", pagination.count);
        } else {
            browserUrl.searchParams.delete("page");
            browserUrl.searchParams.delete("count");
        }

        if (setHistory) {
            let state = {
                tags: tags,
                search: search,
                pagination: pagination
            };

            history.pushState(state, "Search - " + search + " - " + tags, browserUrl.href);
        }

        document.body.scrollTop = 0; // For Safari
        document.documentElement.scrollTop = 0; // For Chrome, Firefox, IE and Opera
        await loadHtml(url);
    }


    loadFor(browserUrl.searchParams.get("tags"), browserUrl.searchParams.get("search"));

    window.onpopstate = (e) => {
        if (e.state.pagination)
            initPageination(e.state.pagination.page, e.state.pagination.count)
        loadFor(e.state.tags, e.state.search, false);
    }

})();

