
(function () {
    const queryString = window.location.search;
    const urlParams = new URLSearchParams(queryString);
    if (urlParams.get("save") !== "1")
        return;
    let ids;
    try {
        ids = JSON.parse(localStorage.getItem("ids"));
        if (!Array.isArray(ids))
            throw "illegal";
    } catch (e) {
        ids = [];
    }

    if(ids.length > 10)
        ids = ids.slice(ids.length - 10)

    let id = urlParams.get("id");
    let name = urlParams.get("name");

    if(!ids.some(i => i.id == id))
        ids.push({ id: id, name: name});

    localStorage.setItem("ids", JSON.stringify(ids));

    window.clearAutosave();
})()