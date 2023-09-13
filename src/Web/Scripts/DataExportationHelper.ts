class DataExportationHelper {
    static async startProgressVerification(componentName: string, routeContext: string) {
        DataExportationHelper.setLoadMessage();

        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext",routeContext)
        urlBuilder.addQueryParameter("gridViewName",componentName)
        urlBuilder.addQueryParameter("dataExportationOperation","checkProgress")
        const url = urlBuilder.build();
        
        var isCompleted : boolean = false;

        while(!isCompleted){
            isCompleted = await DataExportationHelper.checkProgress(url, componentName);
            await sleep(3000);
        }
    }

    static async stopExportation(componentName: string, routeContext: string, stopMessage: string) {
        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext",routeContext)
        urlBuilder.addQueryParameter("gridViewName",componentName)
        urlBuilder.addQueryParameter("dataExportationOperation","stopProcess")
        
        await DataExportationHelper.stopProcess(urlBuilder.build(), stopMessage);
    }


    static openExportPopup(componentName: string, routeContext: string) {
        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext",routeContext)
        urlBuilder.addQueryParameter("gridViewName",componentName)
        urlBuilder.addQueryParameter("dataExportationOperation","showOptions")

        fetch(urlBuilder.build())
            .then(response => response.text())
            .then(data => {
                this.setSettingsHTML(componentName, data)
            })
            .catch(error => {
                console.log(error);
            });
    }

    static startExportation(componentName: string, routeContext: string) {

        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext",routeContext)
        urlBuilder.addQueryParameter("gridViewName",componentName)
        urlBuilder.addQueryParameter("dataExportationOperation","startProcess")

        fetch(urlBuilder.build(),{
            method:"POST",
            body: new FormData(document.querySelector<HTMLFormElement>("form"))
        }).then(response=>response.text()).then(async html => {
            const modalBody = "#data-exportation-modal-" + componentName + " .modal-body ";
            document.querySelector<HTMLElement>(modalBody).innerHTML = html;
            
            listenAllEvents(modalBody);
            await DataExportationHelper.startProgressVerification(componentName,routeContext)
        });
        
    }
    
    static async checkProgress(url, componentName) {
        showSpinnerOnPost = false;

        try {
            const response = await fetch(url);
            const data = await response.json();

            if (data.FinishedMessage) {
                showSpinnerOnPost = true;
                document.querySelector("#data-exportation-modal-" + componentName + " .modal-body").innerHTML = data.FinishedMessage;
                const linkFile = document.querySelector<HTMLLinkElement>("#export_link_" + componentName);
                if (linkFile)
                    linkFile.click();
                return true;
            } else {
                const processStatusElement = document.querySelector<HTMLElement>("#process-status");
                const progressBarElement = document.querySelector<HTMLElement>(".progress-bar");
                const startDateLabelElement = document.querySelector("#start-date-label");
                const processMessageElement = document.querySelector("#process-message");

                if (processStatusElement) {
                    processStatusElement.style.display = "";
                }

                if (progressBarElement) {
                    progressBarElement.style.width = data.PercentProcess + "%";
                    progressBarElement.textContent = data.PercentProcess + "%";
                }

                if (startDateLabelElement) {
                    startDateLabelElement.textContent = data.StartDate;
                }

                if (processMessageElement) {
                    processMessageElement.textContent = data.Message;
                }

                return false;
            }
        } catch (e) {
            showSpinnerOnPost = true;
            const spinnerElement = document.querySelector<HTMLElement>("#data-exportation-spinner-" + componentName);
            
            if(spinnerElement){
                spinnerElement.style.display = "none";
            }
            
            document.querySelector("#data-exportation-modal-" + componentName + " .modal-body").innerHTML = e.message;

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
        const target = document.getElementById('data-exportation-spinner-');
        // @ts-ignore
        var spinner = new Spinner(options).spin(target);
    }
    
    private static setSettingsHTML(componentName, html) {
        const modalBody = document.querySelector("#data-exportation-modal-" + componentName + " .modal-body ");
        modalBody.innerHTML = html;
        listenAllEvents();

        const qtdElement = document.querySelector("#" + componentName + "_totrows");
        if (qtdElement) {
            const totRows = +qtdElement.textContent.replace(/\./g, "");
            if (totRows > 50000) {
                document.querySelector<HTMLElement>("#data-exportation-warning" + componentName).style.display = "block";
            }
        }

        if (bootstrapVersion < 5) {
            $("#data-exportation-modal-" + componentName).modal();
        } else {
            const modal = new bootstrap.Modal(document.querySelector("#data-exportation-modal-" + componentName), {});
            modal.show();
        }
    }
    
    static async stopProcess(url: string, stopMessage: string) {
        document.querySelector<HTMLElement>("#process-status").innerHTML = stopMessage;
        showSpinnerOnPost = false;
        await fetch(url);
    }


    static showOptions(componentName: string, exportType: string) {
        const orientationDiv = document.getElementById(`${componentName}-div-export-orientation`);
        const allDiv = document.getElementById(`${componentName}-div-export-all`);
        const delimiterDiv = document.getElementById(`${componentName}-div-export-delimiter`);
        const firstlineDiv = document.getElementById(`${componentName}-div-export-firstline`);

        if (exportType === "1") { // XLS
            if (orientationDiv) orientationDiv.style.display = "none";
            if (allDiv) allDiv.style.display = "block";
            if (delimiterDiv) delimiterDiv.style.display = "none";
            if (firstlineDiv) firstlineDiv.style.display = "block";
        } else if (exportType === "2") { // PDF
            if (orientationDiv) orientationDiv.style.display = "block";
            if (allDiv) allDiv.style.display = "none";
            if (delimiterDiv) delimiterDiv.style.display = "none";
            if (firstlineDiv) firstlineDiv.style.display = "none";
        } else {
            if (orientationDiv) orientationDiv.style.display = "none";
            if (allDiv) allDiv.style.display = "block";
            if (delimiterDiv) delimiterDiv.style.display = "block";
            if (firstlineDiv) firstlineDiv.style.display = "block";
        }
    }


}