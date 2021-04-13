

(function () {
    let ids;
    try {
        ids = JSON.parse(localStorage.getItem("ids"));
        if (!Array.isArray(ids))
            throw "illegal";
    } catch (e) {
        ids = [];
    }
    if (ids.length === 0)
        return;

    ids = ids.reverse();

    const form = document.getElementById("form");
    const idInput = document.getElementById("ID");

    function onClick(e) {
        id = this.dataset.id;
        
        idInput.value = id;
        form.submit();
        e.preventDefault();
    }

    let list = document.getElementById("idList");

    for (var i = 0; i < ids.length; i++) {
        id = ids[i];
        let link = document.createElement("a");

        link.classList.add("list-group-item", "list-group-item-action");
        link.onclick = onClick;

        let text;
        if (id.name.length > 50)
            text = id.name.substring(47) + "...";
        else
            text = id.name;

        link.innerText = text;
        link.dataset.id = id.id;
        link.href = "#";

        list.append(link);
    }

    document.getElementById("idDiv").classList.remove("d-none");
})();