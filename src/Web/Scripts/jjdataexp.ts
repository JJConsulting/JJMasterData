class JJDataExp {
    static async startProgressVerification(objname) {
        DataExportation.setLoadMessage();

        const form = $("form");
        let formUrl = form.attr("action");
        if (formUrl.includes("?"))
            formUrl += "&t=tableexp";
        else
            formUrl += "?t=tableexp";

        formUrl += "&gridName=" + objname;
        formUrl += "&exptype=checkProgress";
        
        var isCompleted : boolean = false;
        
        while(!isCompleted){
            isCompleted = await DataExportation.checkProgress(formUrl, objname);
            await sleep(3000);
        }
    }

    static async stopProcess(componentName, stopMessage) {
        const form = $("form");
        let url = form.attr("action");
        if (url.includes("?"))
            url += "&t=tableexp";
        else
            url += "?t=tableexp";

        url += "&gridName=" + componentName;
        url += "&exptype=stopProcess";

        await DataExportation.stopExportation(url, stopMessage);
    }
    
    
    static openExportUI(componentName) {
        const frm = $("form");
        let url = frm.attr("action");
        if (url.includes("?"))
            url += "&t=tableexp";
        else
            url += "?t=tableexp";

        url += "&gridName=" + componentName;
        url += "&exptype=showoptions";

        DataExportation.openExportPopup(url, componentName)
    }

    static doExport(objid) {

        var frm = $("form");
        var surl = frm.attr("action");
        if (surl.includes("?"))
            surl += "&t=tableexp";
        else
            surl += "?t=tableexp";

        surl += "&gridName=" + objid;
        surl += "&exptype=export";

        $.ajax({
            async: true,
            type: frm.attr("method"),
            url: surl,
            data: frm.serialize(),
            success: function (data) {
                var modalBody = "#export_modal_" + objid + " .modal-body ";
                $(modalBody).html(data);
                jjloadform(null, modalBody);
                JJDataExp.startProgressVerification(objid)
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
                console.log(textStatus);
                console.log(jqXHR);
            }
        });
    }
}