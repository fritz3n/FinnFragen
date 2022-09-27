
function modifyUrl(u) {
    return u;
}

function htmlForQuestion(q) {
    let tags = "";
    q.tags.forEach(t => {
        tags += `<span class="badge bg-secondary text-white">${escapeHtml(t)}</span>` + "\n";
    });

    return `        
        <li class="list-group-item d-flex flex-row flex-wrap">
            <h5 class="px-3 flex-grow-1 mt-3"><a href="/Questions/Question/${encodeURI(q.shortName)}">${escapeHtml(q.title)}</a></h5>
            <p class="mx-3 mt-3">${new Date(q.answerDate).toLocaleDateString("de-DE")}</p>
            <p class="small col-12 px-3">${escapeHtml(q.synopsis)}</p>
            <div class="d-flex flex-row flex-grow-1">
                <div class="px-3 flex-shrink-1">
                    ${tags}
                </div>
                <p class="text-muted px-3 text-end flex-grow-1 text-nowrap">von ${escapeHtml(q.name)}</p>
            </div>
        </li>`;
}