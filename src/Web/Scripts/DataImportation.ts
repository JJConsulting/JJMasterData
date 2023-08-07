class DataImportation {
    private static insertCount = 0;
    private static updateCount = 0;
    private static deleteCount = 0;
    private static ignoreCount = 0;
    private static errorCount = 0;

    private static setLoadMessage() {
        const options = {
            lines: 13, // The number of lines to draw
            length: 38, // The length of each line
            width: 17, // The line thickness
            radius: 45, // The radius of the inner circle
            scale: 0.2, // Scales overall size of the spinner
            corners: 1, // Corner roundness (0..1)
            color: "#000", // #rgb or #rrggbb or array of colors
            opacity: 0.3, // Opacity of the lines
            rotate: 0, // The rotation offset
            direction: 1, // 1: clockwise, -1: counterclockwise
            speed: 1.2, // Rounds per second
            trail: 62, // Afterglow percentage
            fps: 20, // Frames per second when using setTimeout() as a fallback for CSS
            zIndex: 2e9, // The z-index (defaults to 2000000000)
            className: "spinner", // The CSS class to assign to the spinner
            top: "50%", // Top position relative to parent
            left: "50%", // Left position relative to parent
            shadow: false, // Whether to render a shadow
            hwaccel: false, // Whether to use hardware acceleration
            position: "absolute" // Element positioning

        };
        const target = document.getElementById('impSpin');
        // @ts-ignore
        new Spinner(options).spin(target);
    }

    private static checkProgress(componentName) {
        showWaitOnPost = false;

        let checkProgressUrl = document.getElementById("divProcess").getAttribute("check-progress-url")
        
        let url: string;
        
        if(checkProgressUrl){
            url = checkProgressUrl
        }else{
            let urlBuilder = new UrlBuilder()
            urlBuilder.addQueryParameter("t","ajaxdataimp")
            urlBuilder.addQueryParameter("current_uploadaction","process_check")
            urlBuilder.addQueryParameter("objname",componentName)
            url = urlBuilder.build()
        }
        
        fetch(url, {
            method: 'GET',
            cache: 'no-cache',
            headers: {
                'Content-Type': 'application/json',
            },
        })
            .then(response => response.json())
            .then(result => {
                const divMsgProcess = document.querySelector<HTMLElement>("#divMsgProcess");
                if (divMsgProcess) {
                    divMsgProcess.style.display = "";
                }
                
                const progressBar = document.querySelector<HTMLElement>(".progress-bar");
                if (progressBar) {
                    progressBar.style.width = result.PercentProcess + "%";
                    progressBar.textContent = result.PercentProcess + "%";
                }
                
                const lblResumeLog = document.querySelector<HTMLElement>("#lblResumeLog");
                if (lblResumeLog) {
                    lblResumeLog.textContent = result.Message;
                }
                
                const lblStartDate = document.querySelector<HTMLElement>("#start-date-label");
                if (lblStartDate) {
                    lblStartDate.textContent = result.StartDate;
                }
                if (result.Insert > 0) {
                    document.querySelector<HTMLElement>("#lblInsert").style.display = "";
                    if (result.PercentProcess === 100) {
                        document.querySelector("#lblInsertCount").textContent = result.Insert;
                    } else {
                        jjutil.animateValue("lblInsertCount", DataImportation.insertCount, result.Insert, 1000);
                    }
                    DataImportation.insertCount = result.Insert;
                }

                if (result.Update > 0) {
                    document.querySelector<HTMLElement>("#lblUpdate").style.display = "";
                    if (result.PercentProcess === 100) {
                        document.querySelector("#lblUpdateCount").textContent = result.Update;
                    } else {
                        jjutil.animateValue("lblUpdateCount", DataImportation.updateCount, result.Update, 1000);
                    }
                    DataImportation.updateCount = result.Update;
                }

                if (result.Delete > 0) {
                    document.querySelector<HTMLElement>("#lblDelete").style.display = "";
                    if (result.PercentProcess === 100) {
                        document.querySelector("#lblDeleteCount").textContent = result.Delete;
                    } else {
                        jjutil.animateValue("lblDeleteCount", DataImportation.deleteCount, result.Delete, 1000);
                    }
                    DataImportation.deleteCount = result.Delete;
                }

                if (result.Ignore > 0) {
                    document.querySelector<HTMLElement>("#lblIgnore").style.display = "";
                    if (result.PercentProcess === 100) {
                        document.querySelector("#lblIgnoreCount").textContent = result.Ignore;
                    } else {
                        jjutil.animateValue("lblIgnoreCount", DataImportation.ignoreCount, result.Ignore, 1000);
                    }
                    DataImportation.ignoreCount = result.Ignore;
                }

                if (result.Error > 0) {
                    document.querySelector<HTMLElement>("#lblError").style.display = "";
                    if (result.PercentProcess === 100) {
                        document.querySelector("#lblErrorCount").textContent = result.Error;
                    } else {
                        jjutil.animateValue("lblErrorCount", DataImportation.errorCount, result.Error, 1000);
                    }
                    DataImportation.errorCount = result.Error;
                }

                if (!result.IsProcessing) {
                    document.querySelector<HTMLInputElement>("#current_uploadaction").value = "process_finished";
                    setTimeout(function () {
                        document.querySelector("form").dispatchEvent(new Event("submit"));
                    }, 1000);
                }
            })
            .catch(error => {
                console.error('Error fetching data:', error);
            });
    }
    static startProcess(objname) {
        $(document).ready(function () {
            DataImportation.setLoadMessage();

            setInterval(function () {
                DataImportation.checkProgress(objname);
            }, 3000);
        });
    }

    static stopProcess(componentName, stopLabel) {
        showWaitOnPost = false;

        let stopProcessUrl = document.getElementById("divProcess").getAttribute("stop-process-url")

        let url: string;

        if(stopProcessUrl){
            url = stopProcessUrl
        }else{
            let urlBuilder = new UrlBuilder()
            urlBuilder.addQueryParameter("t","ajaxdataimp")
            urlBuilder.addQueryParameter("current_uploadaction","process_check")
            urlBuilder.addQueryParameter("objname",componentName)
            url = urlBuilder.build()
        }


        fetch(url).then(response=>response.json()).then(data=>{
            if(data.isProcessing === false){
                document.getElementById("divMsgProcess").innerHTML = stopLabel;
            }
        });
    }

    static addPasteListener() {
        $(document).ready(function () {
            document.addEventListener("paste", (e: Event) => {
                var pastedText = undefined;
                if (window.clipboardData && window.clipboardData.getData) { // IE 
                    pastedText = window.clipboardData.getData("Text");
                } else if (e.clipboardData && e.clipboardData.getData) {
                    pastedText = e.clipboardData.getData("text/plain");
                }
                e.preventDefault();
                if (pastedText != undefined) {

                    $("#current_uploadaction").val("posted_past_text");
                    $("#pasteValue").val(pastedText);
                    $("form:first").trigger("submit");
                }
                return false;
            });
        });
    }

}
