
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
            <p class="small col-12 px-3">${escapeHtml(q.synopsis)}</p>
            <div class="px-3 flex-grow-1">
                ${tags}
            </div>
            <p class="text-muted px-3 text-right">von ${escapeHtml(q.name)}</p>
        </li>`;
}