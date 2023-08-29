function loadAuditLog(componentName, logId, url = null) {
    $("#sortable-grid a").removeClass("active");

    if (logId != "")
        $("#" + logId).addClass("active");

    document.querySelector<HTMLInputElement>('#audit-log-id-' + componentName).value = logId;

    if(url == null || url.length == 0){
        let builder = new UrlBuilder();
        builder.addQueryParameter("context","htmlContent");
        url = builder.build();
    }
    
    postFormValues({
        url:url,
        success: function (data){
            document.getElementById("auditlogview-panel-" + componentName).innerHTML = data;
        }
    })
}