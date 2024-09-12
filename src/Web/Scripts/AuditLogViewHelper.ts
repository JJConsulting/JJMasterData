class AuditLogViewHelper {

    static viewAuditLog(componentName: string, id: string) {
        const auditLogIdInput = document.getElementById("audit-log-id-" + componentName) as HTMLInputElement;
        const form = getMasterDataForm();

        if (auditLogIdInput) {
            auditLogIdInput.value = id;
        }

        if (form) {
            form.submit();
        }
    }

    
    static loadAuditLog(componentName: string, logId: string, routeContext:string) {
        $("#sortable-grid a").removeClass("active");

        if (logId != "")
            $("#" + logId).addClass("active");

        document.querySelector<HTMLInputElement>('#audit-log-id-' + componentName).value = logId;


        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext",routeContext);

        postFormValues({
            url: urlBuilder.build(),
            success: function (data){
                document.getElementById("auditlogview-panel-" + componentName).innerHTML = data;
            }
        })
    }
}