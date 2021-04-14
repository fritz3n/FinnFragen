
function modifyUrl(u) {
    if (u.indexOf("?") >= 0) {
        u += "&all=true";
    } else {
        u += "?all=true";
    }

    return u;
}

function htmlForQuestion(q) {
    let tags = "";
    let url = window.location.href;
    q.tags.forEach(t => {
        tags += `<span class="badge bg-secondary text-white">${escapeHtml(t)}</span>` + "\n";
    });

    return `<li class="list-group-item d-flex flex-row flex-wrap">
                    <div class="d-flex col-12 m-0 p-0 ms-3">
                        <div class="pl-3 flex-grow-1 mt-3">
                            <h5 class="d-inline w-auto">
                                <a href="/Status/Status/${encodeURIComponent(q.restricted.id)}">${escapeHtml(q.title)}</a>
                            </h5>
                            <a href="/Questions/Question/${encodeURIComponent(q.shortName)}"><i class="fas fa-external-link-alt"></i></a>
                        </div>
                        <div class="mt-3 px-2">
                            <span class="badge bg-${q.restricted.lastActionColor}">${q.restricted.lastAction}</span>
                        </div>
                        <div class="d-inline pt-1">
                            <div class="btn-group btn-group-sm mt-2" role="group" aria-label="First group">
                            <a href="/Status/Answer/${q.restricted.id}?ret=${encodeURIComponent(url)}" type="button" class="btn btn-outline-success"><i class="fas fa-check-double"></i></a>
                            <a href="/Status/Block/${q.restricted.id}?ret=${encodeURIComponent(url)}" type="button" class="btn btn-outline-danger"><i class="fas fa-ban"></i></i></a>
                            <a href="/Status/Delete/${q.restricted.id}?ret=${encodeURIComponent(url)}" type="button" class="btn btn-outline-danger"><i class="far fa-trash-alt"></i></a>
                            </div>
                        </div>
                        </div>
                    <p class="small col-12 px-3">${escapeHtml(q.synopsis)}</p>
                    <div class="px-3 flex-grow-1 ">
                        ${tags}
                    </div>
                    <p class="text-muted px-3text-right">from ${escapeHtml(q.name)} on ${new Date(q.restricted.lastActionDate).toLocaleDateString("de-DE")}</p>
                </li>

`;
}