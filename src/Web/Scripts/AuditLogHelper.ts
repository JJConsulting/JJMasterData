class AuditLogHelper{

    static viewAuditLog(componentName: string, id: string) {
        const auditLogIdInput = document.getElementById("audit-log-id-" + componentName) as HTMLInputElement;
        const form = document.querySelector<HTMLFormElement>("form");

        if (auditLogIdInput) {
            auditLogIdInput.value = id;
        }

        if (form) {
            form.dispatchEvent(new Event("submit", { bubbles: true, cancelable: false    }));
        }
    }

    
    static loadAuditLog(componentName, logId, url = null) {
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
}