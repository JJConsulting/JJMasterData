function loadAuditLog(componentName, logId, url = null) {
    $("#sortable_grid a").removeClass("active");

    if (logId != "")
        $("#" + logId).addClass("active");

    document.querySelector<HTMLInputElement>('#logId-' + componentName).value = logId;

    if(url == null || url.length == 0){
        let builder = new UrlBuilder();
        builder.addQueryParameter("t","ajax");
        url = builder.build();
    }
    
    fetch(url, {
        method: "POST",
        body: new FormData(document.querySelector<HTMLFormElement>("form"))
    }).then(response => response.text()).then(data => {
            document.getElementById("auditlogview-panel-" + componentName).innerHTML = data;
    })
}