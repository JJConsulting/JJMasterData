class JJDataExp {
    static setLoadMessage() {
        const options = {
            lines: 13 // The number of lines to draw
            , length: 38 // The length of each line
            , width: 17 // The line thickness
            , radius: 45 // The radius of the inner circle
            , scale: 0.2 // Scales overall size of the spinner
            , corners: 1 // Corner roundness (0..1)
            , color: "#000" // #rgb or #rrggbb or array of colors
            , opacity: 0.3 // Opacity of the lines
            , rotate: 0 // The rotation offset
            , direction: 1 // 1: clockwise, -1: counterclockwise
            , speed: 1.2 // Rounds per second
            , trail: 62 // Afterglow percentage
            , fps: 20 // Frames per second when using setTimeout() as a fallback for CSS
            , zIndex: 2e9 // The z-index (defaults to 2000000000)
            , className: "spinner" // The CSS class to assign to the spinner
            , top: "50%" // Top position relative to parent
            , left: "50%" // Left position relative to parent
            , shadow: false // Whether to render a shadow
            , hwaccel: false // Whether to use hardware acceleration
            , position: "absolute" // Element positioning

        }
        var target = document.getElementById('impSpin');
        // @ts-ignore
        var spinner = new Spinner(options).spin(target);
    }

    static async checkProgress(objname) {
        showWaitOnPost = false;

        const form = $("form");
        let formUrl = form.attr("action");
        if (formUrl.includes("?"))
            formUrl += "&t=tableexp";
        else
            formUrl += "?t=tableexp";

        formUrl += "&gridName=" + objname;
        formUrl += "&exptype=checkProgress";
        
        try{
            const response = await fetch(formUrl);
            const data = await response.json();

            if (data.FinishedMessage) {
                showWaitOnPost = true;
                $("#export_modal_" + objname + " .modal-body").html(data.FinishedMessage);
                $("#dataexp_spinner_" + objname).hide();
                const linkFile = $("#export_link_" + objname)[0];
                if (linkFile)
                    linkFile.click();

                return true;
            } else {
                $("#divMsgProcess").css("display", "");
                $(".progress-bar").css("width", data.PercentProcess + "%").text(data.PercentProcess + "%");
                $("#lblStartDate").text(data.StartDate);
                $("#lblResumeLog").text(data.Message);

                return false;
            }
        }
        catch(e){
            showWaitOnPost = true;
            $("#dataexp_spinner_" + objname).hide();
            $("#export_modal_" + objname + " .modal-body").html(e.message);
            
            return false;
        }

    }

    static async startProcess(objname) {
        JJDataExp.setLoadMessage();
        
        var isCompleted : boolean = false;
        
        while(!isCompleted){
            isCompleted = await JJDataExp.checkProgress(objname);
            await sleep(3000);
        }
    }

    static async stopProcess(objid, stopStr) {
        $("#divMsgProcess").html(stopStr);
        showWaitOnPost = false;
        var frm = $("form");
        var surl = frm.attr("action");
        if (surl.includes("?"))
            surl += "&t=tableexp";
        else
            surl += "?t=tableexp";

        surl += "&gridName=" + objid;
        surl += "&exptype=stopProcess";

        await fetch(surl);
    }

    
    private static setSettingsHTML(componentName, html){
        const modalBody = "#export_modal_" + componentName + " .modal-body ";
        $(modalBody).html(html);
        jjloadform(null, modalBody);

        const qtdElement = $("#" + componentName + "_totrows");
        if (qtdElement.length > 0) {
            const totRows = +qtdElement.text().replace(".", "").replace(".", "").replace(".", "").replace(".", "");
            if (totRows > 50000)
                $("#warning_exp_" + componentName).show();
        }

        if (bootstrapVersion < 5) {
            $("#export_modal_" + componentName).modal();
        } else {
            const modal = new bootstrap.Modal("#export_modal_" + componentName, {});
            modal.show();
        }
    }
    
    static openExportPopup(url: string, componentName: string) {
        fetch(url)
            .then(response => response.text())
            .then(data => {
                this.setSettingsHTML(componentName, data)
            })
            .catch(error => {
                console.log(error);
            });
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

        this.openExportPopup(url, componentName)
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
                //JJDataExp.startProcess(objid);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
                console.log(textStatus);
                console.log(jqXHR);
            }
        });
    }
}