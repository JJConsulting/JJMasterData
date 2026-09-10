class DataExportationHelper {
    private static progressVerifications = new Set<string>();

    static async startProgressVerification(componentName: string, routeContext: string) {
        DataExportationHelper.setSpinner();
        DataExportationHelper.setActionProcessing(componentName, true);

        if (DataExportationHelper.progressVerifications.has(componentName))
            return;

        DataExportationHelper.progressVerifications.add(componentName);

        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext",routeContext)
        urlBuilder.addQueryParameter("gridViewName",componentName)
        urlBuilder.addQueryParameter("dataExportationOperation","checkProgress")
        urlBuilder.addQueryParameter("jobId",DataExportationHelper.getJobId(componentName))
        const url = urlBuilder.build();
        
        var isCompleted : boolean = false;

        try {
            while(!isCompleted){
                isCompleted = await DataExportationHelper.checkProgress(url, componentName);
                if (!isCompleted)
                    await sleep(3000);
            }
        } finally {
            DataExportationHelper.progressVerifications.delete(componentName);
            DataExportationHelper.setActionProcessing(componentName, false);
        }
    }

    static async stopExportation(componentName: string, routeContext: string, stopMessage: string) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext",routeContext)
        urlBuilder.addQueryParameter("gridViewName",componentName)
        urlBuilder.addQueryParameter("dataExportationOperation","stopProcess")
        urlBuilder.addQueryParameter("jobId",DataExportationHelper.getJobId(componentName))
        
        await DataExportationHelper.stopProcess(urlBuilder.build(), stopMessage);
    }


    static openExportPopup(componentName: string, routeContext: string) {
        const urlBuilder = new UrlBuilder();
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

        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext",routeContext)
        urlBuilder.addQueryParameter("gridViewName",componentName)
        urlBuilder.addQueryParameter("dataExportationOperation","startProcess")
        const requestOptions = getRequestOptions();
        fetch(urlBuilder.build(), requestOptions)
            .then(response => response.text()).then(async html => {
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
    
    private static setSpinner() {
        const target = document.getElementById('data-exportation-spinner-');
        if (!target || target.childElementCount > 0)
            return;

        if(bootstrapVersion < 5){
            const options = {
                className: "spinner",
                color: "#000",
                corners: 1,
                direction: 1,
                fps: 20,
                hwaccel: false,
                left: "50%",
                length: 38,
                lines: 13,
                opacity: 0.3,
                position: "absolute",
                radius: 45,
                rotate: 0,
                scale: 0.2,
                shadow: false,
                speed: 1.2,
                top: "50%",
                trail: 62,
                width: 17,
                zIndex: 2e9
            };
            // @ts-ignore
            new Spinner(options).spin(target);
        }
        else{
            const spinnerDiv = document.createElement('div');
            spinnerDiv.classList.add('spinner-border','text-primary','spinner-border-lg',);
            spinnerDiv.setAttribute('role', 'status');
            const spanElement = document.createElement('span');
            spanElement.classList.add('visually-hidden');
            spanElement.textContent = 'Loading...';
            spinnerDiv.appendChild(spanElement);
            target.append(spinnerDiv);
        }
    }

    private static setActionProcessing(componentName: string, isProcessing: boolean) {
        const button = Array.from(document.querySelectorAll<HTMLElement>('[onclick*="DataExportationHelper.openExportPopup"]'))
            .find(element => element.getAttribute('onclick')?.includes(`'${componentName}'`));
        if (!button)
            return;

        button.setAttribute('aria-busy', isProcessing.toString());
        const indicatorId = `data-exportation-action-spinner-${componentName}`;
        const existingIndicator = document.getElementById(indicatorId);
        if (!isProcessing) {
            existingIndicator?.remove();
            button.querySelector('.data-exportation-action-indicator')?.remove();
            return;
        }

        if (existingIndicator || button.querySelector('.data-exportation-action-indicator'))
            return;

        const indicator = document.createElement('span');
        indicator.id = indicatorId;
        indicator.classList.add('spinner-border', 'data-operation-action-indicator', 'data-exportation-action-indicator');
        indicator.setAttribute('role', 'status');
        indicator.setAttribute('aria-hidden', 'true');
        button.appendChild(indicator);
    }

    
    private static setSettingsHTML(componentName, html) {
        const modalBody: HTMLElement = document.querySelector("#data-exportation-modal-" + componentName + " .modal-body ");
        HTMLHelper.setInnerHTML(modalBody,html)
        
        const qtdElement = document.querySelector("#" + componentName + "-total-of-records");
        if (qtdElement) {
            const totRows = +qtdElement.textContent.replace(/\./g, "");
            const warningElement = document.querySelector<HTMLElement>("#data-exportation-warning" + componentName);
            if (totRows > 50000 && warningElement)
                warningElement.style.display = "block";
        }
        
        if (bootstrapVersion < 5) {
            $("#data-exportation-modal-" + componentName).modal();
        } else {
            const modal = bootstrap.Modal.getOrCreateInstance(document.querySelector("#data-exportation-modal-" + componentName), {});
            modal.show();
        }

        listenAllEvents("#data-exportation-modal-" + componentName);
    }
    
    static async stopProcess(url: string, stopMessage: string) {
        document.querySelector<HTMLElement>("#process-status").innerHTML = stopMessage;
        showSpinnerOnPost = false;
        await fetch(url);
    }


    static showOptions(componentName: string, exportType: string) {
        document.querySelectorAll<HTMLElement>(".data-export-format-option").forEach(element => {
            element.style.display = element.dataset.exportFormat === exportType ? "block" : "none";
        });
    }

    private static getJobId(componentName: string): string {
        return document.querySelector<HTMLInputElement>(`#${componentName}-export-job-id`)?.value ?? "";
    }


}
