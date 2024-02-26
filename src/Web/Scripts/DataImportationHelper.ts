class DataImportationHelper {
    private static insertCount = 0;
    private static updateCount = 0;
    private static deleteCount = 0;
    private static ignoreCount = 0;
    private static errorCount = 0;

    private static pasteEventListener;

    private static setSpinner() {
        const target = document.getElementById('data-importation-spinner');
        
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

    private static checkProgress(componentName, importationRouteContext, gridRouteContext, intervalId: number) {
        showSpinnerOnPost = false;

        const urlBuilder = new UrlBuilder()
        urlBuilder.addQueryParameter("routeContext", importationRouteContext)
        urlBuilder.addQueryParameter("dataImportationOperation", "checkProgress")
        urlBuilder.addQueryParameter("componentName", componentName)
        const url = urlBuilder.build()
        
        
        
        fetch(url, {
            method: 'GET',
            cache: 'no-cache',
            headers: {
                'Content-Type': 'application/json',
            },
        })
            .then(response => response.json())
            .then(result => {
                const processMessageDiv = document.querySelector<HTMLElement>("#process-status");
                if (processMessageDiv) {
                    processMessageDiv.style.display = "";
                }

                const progressBar = document.querySelector<HTMLElement>(".progress-bar");
                if (progressBar) {
                    progressBar.style.width = result.PercentProcess + "%";
                    progressBar.textContent = result.PercentProcess + "%";
                }

                const processMessage = document.querySelector<HTMLElement>("#process-message");
                if (processMessage) {
                    processMessage.textContent = result.Message;
                }

                const startDateLabel = document.querySelector<HTMLElement>("#start-date-label");
                if (startDateLabel) {
                    startDateLabel.textContent = result.StartDate;
                }
                if (result.Insert > 0) {
                    document.querySelector<HTMLElement>("#lblInsert").style.display = "";
                    if (result.PercentProcess === 100) {
                        document.querySelector("#lblInsertCount").textContent = result.Insert;
                    } else {
                        jjutil.animateValue("lblInsertCount", DataImportationHelper.insertCount, result.Insert, 1000);
                    }
                    DataImportationHelper.insertCount = result.Insert;
                }

                if (result.Update > 0) {
                    document.querySelector<HTMLElement>("#lblUpdate").style.display = "";
                    if (result.PercentProcess === 100) {
                        document.querySelector("#lblUpdateCount").textContent = result.Update;
                    } else {
                        jjutil.animateValue("lblUpdateCount", DataImportationHelper.updateCount, result.Update, 1000);
                    }
                    DataImportationHelper.updateCount = result.Update;
                }

                if (result.Delete > 0) {
                    document.querySelector<HTMLElement>("#lblDelete").style.display = "";
                    if (result.PercentProcess === 100) {
                        document.querySelector("#lblDeleteCount").textContent = result.Delete;
                    } else {
                        jjutil.animateValue("lblDeleteCount", DataImportationHelper.deleteCount, result.Delete, 1000);
                    }
                    DataImportationHelper.deleteCount = result.Delete;
                }

                if (result.Ignore > 0) {
                    document.querySelector<HTMLElement>("#lblIgnore").style.display = "";
                    if (result.PercentProcess === 100) {
                        document.querySelector("#lblIgnoreCount").textContent = result.Ignore;
                    } else {
                        jjutil.animateValue("lblIgnoreCount", DataImportationHelper.ignoreCount, result.Ignore, 1000);
                    }
                    DataImportationHelper.ignoreCount = result.Ignore;
                }

                if (result.Error > 0) {
                    document.querySelector<HTMLElement>("#lblError").style.display = "";
                    if (result.PercentProcess === 100) {
                        document.querySelector("#lblErrorCount").textContent = result.Error;
                    } else {
                        jjutil.animateValue("lblErrorCount", DataImportationHelper.errorCount, result.Error, 1000);
                    }
                    DataImportationHelper.errorCount = result.Error;
                }

                if (!result.IsProcessing) {
                    clearInterval(intervalId)
                    
                    const urlBuilder = new UrlBuilder();
                    urlBuilder.addQueryParameter("routeContext", importationRouteContext)
                    urlBuilder.addQueryParameter("dataImportationOperation", "log")

                    postFormValues({
                        url: urlBuilder.build(), success: html => {
                            document.querySelector<HTMLInputElement>("#" + componentName).innerHTML = html;
                            GridViewHelper.refreshGrid(componentName.replace("-importation",String()), gridRouteContext)
                        }
                    })
                }
            })
            .catch(error => {
                console.error('Error fetching data:', error);
            });
    }

    static show(componentName: string, modalTitle: string, routeContext: string, gridRouteContext: string) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        const requestOptions = getRequestOptions();
        
        DataImportationModal.getInstance().showUrl({
            url: urlBuilder.build(),
            requestOptions: requestOptions
        }, modalTitle, ModalSize.ExtraLarge).then(_ => {
            DataImportationHelper.addPasteListener(componentName, routeContext, gridRouteContext);
            UploadAreaListener.listenFileUpload()
        })
    }
    
    static back(componentName: string, routeContext: string, gridRouteContext: string){
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        postFormValues({url:urlBuilder.build(), success: html => {
            document.querySelector<HTMLInputElement>("#" + componentName).innerHTML = html;
            DataImportationHelper.addPasteListener(componentName, routeContext, gridRouteContext);
            UploadAreaListener.listenFileUpload()
        }})
    }

    static showLog(componentName, routeContext) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        urlBuilder.addQueryParameter("dataImportationOperation", "log");
        postFormValues({
            url: urlBuilder.build(), success: html => {
                DataImportationHelper.removePasteListener();
                document.querySelector<HTMLInputElement>("#" + componentName).innerHTML = html;
            }
        })
    }

    static startProgressVerification(componentName, routeContext, gridRouteContext) {

        DataImportationHelper.setSpinner();

        let intervalId = setInterval(function () {
            DataImportationHelper.checkProgress(componentName, routeContext, gridRouteContext, intervalId);
        }, 3000);

    }

    static help(componentName, routeContext) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        urlBuilder.addQueryParameter("dataImportationOperation", "help");
        postFormValues({
            url: urlBuilder.build(), success: html => {
                DataImportationHelper.removePasteListener();
                document.querySelector<HTMLInputElement>("#" + componentName).innerHTML = html;
            }
        })
    }

    static stop(componentName, routeContext, stopLabel) {
        showSpinnerOnPost = false;

        const urlBuilder = new UrlBuilder()
        urlBuilder.addQueryParameter("routeContext", routeContext)
        urlBuilder.addQueryParameter("dataImportationOperation", "stop")
        urlBuilder.addQueryParameter("componentName", componentName)
        const url = urlBuilder.build()

        fetch(url).then(response => response.json()).then(data => {
            if (data.isProcessing === false) {
                document.getElementById("divMsgProcess").innerHTML = stopLabel;
            }
        });
    }

    static addPasteListener(componentName: string, routeContext: string, gridRouteContext: string) {
        DataImportationHelper.pasteEventListener = function onPaste(e) {
            DataImportationHelper.removePasteListener();
            let pastedText = undefined;
            if (window.clipboardData && window.clipboardData.getData) { // IE 
                pastedText = window.clipboardData.getData("Text");
            } else if (e.clipboardData && e.clipboardData.getData) {
                pastedText = e.clipboardData.getData("text/plain");
            }
            e.preventDefault();
            if (pastedText != undefined) {
                document.querySelector<HTMLInputElement>("#pasteValue").value = pastedText;
                
                const urlBuilder = new UrlBuilder();
                urlBuilder.addQueryParameter("routeContext", routeContext)
                urlBuilder.addQueryParameter("dataImportationOperation", "processPastedText")
                const requestOptions = getRequestOptions();

                postFormValues({
                    url: urlBuilder.build(), success: html => {
                        document.querySelector<HTMLInputElement>("#" + componentName).innerHTML = html;
                        DataImportationHelper.startProgressVerification(componentName, routeContext, gridRouteContext);
                    }
                })
            }
            return false;
        }

        document.addEventListener("paste", DataImportationHelper.pasteEventListener, {once: true});
    }
    
    static uploadCallback(componentName: string, routeContext: string, gridRouteContext: string){
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext)
        urlBuilder.addQueryParameter("dataImportationOperation", "loading")

        postFormValues({
            url: urlBuilder.build(),
            success: html => {
                document.querySelector<HTMLInputElement>("#" + componentName).innerHTML = html;
                DataImportationHelper.startProgressVerification(componentName, routeContext, gridRouteContext)
            }
        })
    }

    static removePasteListener() {
        if (DataImportationHelper.pasteEventListener) {
            document.removeEventListener("paste", DataImportationHelper.pasteEventListener);
        }
    }
}

