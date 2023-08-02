class DataExportation{
    static async startProgressVerificationAtSamePage(componentName) {
        DataExportation.setLoadMessage();

        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("t","tableExp")
        urlBuilder.addQueryParameter("gridName",componentName)
        urlBuilder.addQueryParameter("exptype","checkProgress")
        
        var isCompleted : boolean = false;

        while(!isCompleted){
            isCompleted = await DataExportation.checkProgress(urlBuilder.build(), componentName);
            await sleep(3000);
        }
    }

    static async stopProcessAtSamePage(componentName, stopMessage) {
        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("t","tableExp")
        urlBuilder.addQueryParameter("gridName",componentName)
        urlBuilder.addQueryParameter("exptype","stopProcess")
        
        await DataExportation.stopExportation(urlBuilder.build(), stopMessage);
    }


    static openExportPopupAtSamePage(componentName) {
        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("t","tableExp")
        urlBuilder.addQueryParameter("gridName",componentName)
        urlBuilder.addQueryParameter("exptype","showOptions")

        DataExportation.openExportPopup(urlBuilder.build(), componentName)
    }

    static startExportationAtSamePage(componentName) {

        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("t","tableExp")
        urlBuilder.addQueryParameter("gridName",componentName)
        urlBuilder.addQueryParameter("exptype","export")

        fetch(urlBuilder.build(),{
            method:"POST",
            body: new FormData(document.querySelector<HTMLFormElement>("form"))
        }).then(response=>response.text()).then(async html => {
            const modalBody = "#export_modal_" + componentName + " .modal-body ";
            document.querySelector<HTMLElement>(modalBody).innerHTML = html;
            
            loadJJMasterData(null, modalBody);
            await DataExportation.startProgressVerificationAtSamePage(componentName)
        });
        
    }
    
    static async checkProgress(url, componentName) {
        showWaitOnPost = false;

        try {
            const response = await fetch(url);
            const data = await response.json();

            if (data.FinishedMessage) {
                showWaitOnPost = true;
                document.querySelector("#export_modal_" + componentName + " .modal-body").innerHTML = data.FinishedMessage;
                const linkFile = document.querySelector<HTMLLinkElement>("#export_link_" + componentName);
                if (linkFile)
                    linkFile.click();

                return true;
            } else {
                document.querySelector<HTMLElement>("#divMsgProcess").style.display = "";
                document.querySelector<HTMLElement>(".progress-bar").style.width = data.PercentProcess + "%";
                document.querySelector(".progress-bar").textContent = data.PercentProcess + "%";
                document.querySelector("#lblStartDate").textContent = data.StartDate;
                document.querySelector("#lblResumeLog").textContent = data.Message;

                return false;
            }
        } catch (e) {
            showWaitOnPost = true;
            document.querySelector<HTMLElement>("#dataexp_spinner_" + componentName).style.display = "none";
            document.querySelector("#export_modal_" + componentName + " .modal-body").innerHTML = e.message;

            return false;
        }
    }
    
    
    
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
        const target = document.getElementById('exportationSpinner');
        // @ts-ignore
        var spinner = new Spinner(options).spin(target);
    }
    
    private static setSettingsHTML(componentName, html) {
        const modalBody = document.querySelector("#export_modal_" + componentName + " .modal-body ");
        modalBody.innerHTML = html;
        loadJJMasterData(null);

        const qtdElement = document.querySelector("#" + componentName + "_totrows");
        if (qtdElement) {
            const totRows = +qtdElement.textContent.replace(/\./g, "");
            if (totRows > 50000) {
                document.querySelector<HTMLElement>("#warning_exp_" + componentName).style.display = "block";
            }
        }

        if (bootstrapVersion < 5) {
            $("#export_modal_" + componentName).modal();
        } else {
            const modal = new bootstrap.Modal(document.querySelector("#export_modal_" + componentName), {});
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
    static startExportation(startExportationUrl,checkProgressUrl, componentName) {
        
        const form = document.querySelector("form");
        
        fetch(startExportationUrl, {
            method: "POST",
            body: new FormData(form)
        })
            .then(response => {
                if (response.ok) {
                    return response.text();
                } else {
                    throw new Error("Request failed with status: " + response.status);
                }
            })
            .then(data => {
                const modalBody = document.querySelector("#export_modal_" + componentName + " .modal-body");
                modalBody.innerHTML = data;
                loadJJMasterData();
                DataExportation.startProgressVerification(checkProgressUrl,componentName)
 
            })
            .catch(error => {
                console.log(error);
            });
    }


    static async stopExportation(url: string, stopMessage: string) {
        document.querySelector<HTMLElement>("#divMsgProcess").innerHTML = stopMessage;
        showWaitOnPost = false;
        await fetch(url);
    }


    static async startProgressVerification(url, componentName) {
        DataExportation.setLoadMessage();

        var isCompleted : boolean = false;

        while(!isCompleted){
            isCompleted = await DataExportation.checkProgress(url,componentName);
            await sleep(3000);
        }
    }
}