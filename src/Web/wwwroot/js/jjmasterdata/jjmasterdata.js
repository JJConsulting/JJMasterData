var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
class ActionData {
}
class ActionManager {
    static executeSqlCommand(componentName, rowId, confirmMessage) {
        if (confirmMessage) {
            var result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }
        document.querySelector("#grid-view-action-" + componentName).value = "";
        document.querySelector("#grid-view-row-" + componentName).value = rowId;
        const formViewActionMapElement = document.querySelector("#form-view-action-map-" + componentName);
        if (formViewActionMapElement) {
            formViewActionMapElement.value = "";
        }
        document.querySelector("form").dispatchEvent(new Event("submit"));
    }
    static executeRedirectAction(componentName, routeContext, encryptedActionMap, confirmationMessage) {
        if (confirmationMessage) {
            const result = confirm(confirmationMessage);
            if (!result) {
                return false;
            }
        }
        const currentFormActionInput = document.querySelector("#form-view-action-map-" + componentName);
        currentFormActionInput.value = encryptedActionMap;
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        urlBuilder.addQueryParameter("componentName", componentName);
        const url = urlBuilder.build();
        this.executeUrlRedirect(url);
        return true;
    }
    static executeUrlRedirect(url) {
        postFormValues({
            url: url,
            success: (data) => {
                if (data.urlAsPopUp) {
                    if (data.isIframe) {
                        defaultModal.showIframe(data.urlRedirect, data.popUpTitle);
                    }
                    else {
                        defaultModal.showUrl(data.urlRedirect, data.popUpTitle);
                    }
                }
                else {
                    window.location.href = data.urlRedirect;
                }
            }
        });
    }
    static executeActionData(actionData) {
        const { componentName, actionMap, modalTitle, modalRouteContext, gridRouteContext, confirmationMessage } = actionData;
        if (confirmationMessage) {
            if (!confirm(confirmationMessage)) {
                return false;
            }
        }
        const gridViewActionInput = document.querySelector("#grid-view-action-" + componentName);
        const formViewActionInput = document.querySelector("#form-view-action-map-" + componentName);
        if (gridViewActionInput) {
            gridViewActionInput.value = null;
        }
        if (formViewActionInput) {
            formViewActionInput.value = actionMap;
        }
        let form = document.querySelector("form");
        if (!form) {
            return;
        }
        if (modalRouteContext) {
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("routeContext", modalRouteContext);
            const modal = new Modal();
            modal.modalId = componentName + "-modal";
            modal.showUrl({
                url: urlBuilder.build(), requestOptions: {
                    method: "POST",
                    body: new FormData(document.querySelector("form"))
                }
            }, modalTitle).then(function (data) {
                listenAllEvents("#" + modal.modalId + " ");
                if (typeof data === "object") {
                    if (data.closeModal) {
                        modal.remove();
                        GridViewHelper.refresh(componentName, gridRouteContext);
                    }
                }
            });
        }
        else {
            form.requestSubmit();
        }
    }
    static executeAction(actionDataJson) {
        const actionData = JSON.parse(actionDataJson);
        return this.executeActionData(actionData);
    }
    static hideActionModal(componentName) {
        const modal = new Modal();
        modal.modalId = componentName + "-modal";
        modal.hide();
    }
}
class AuditLogViewHelper {
    static viewAuditLog(componentName, id) {
        const auditLogIdInput = document.getElementById("audit-log-id-" + componentName);
        const form = document.querySelector("form");
        if (auditLogIdInput) {
            auditLogIdInput.value = id;
        }
        if (form) {
            form.requestSubmit();
        }
    }
    static loadAuditLog(componentName, logId, routeContext) {
        $("#sortable-grid a").removeClass("active");
        if (logId != "")
            $("#" + logId).addClass("active");
        document.querySelector('#audit-log-id-' + componentName).value = logId;
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        postFormValues({
            url: urlBuilder.build(),
            success: function (data) {
                document.getElementById("auditlogview-panel-" + componentName).innerHTML = data;
            }
        });
    }
}
class CalendarListener {
    static listen(prefixSelector = String()) {
        $(prefixSelector + ".jjform-datetime").flatpickr({
            enableTime: true,
            wrap: true,
            allowInput: true,
            altInput: false,
            time_24hr: true,
            dateFormat: localeCode === "pt" ? "d/m/Y H:i" : "m/d/Y H:i",
            onOpen: function (selectedDates, dateStr, instance) {
                if (instance.input.getAttribute("autocompletePicker") == 1) {
                    instance.setDate(Date.now());
                }
            },
            locale: localeCode
        });
        $(prefixSelector + ".jjform-date").flatpickr({
            enableTime: false,
            wrap: true,
            allowInput: true,
            altInput: false,
            dateFormat: localeCode === "pt" ? "d/m/Y" : "m/d/Y",
            onOpen: function (selectedDates, dateStr, instance) {
                if (instance.input.getAttribute("autocompletePicker") == 1) {
                    instance.setDate(Date.now());
                }
            },
            locale: localeCode
        });
        $(prefixSelector + ".jjform-hour").flatpickr({
            enableTime: true,
            wrap: true,
            noCalendar: true,
            allowInput: true,
            altInput: false,
            dateFormat: "H:i",
            time_24hr: true,
            onOpen: function (selectedDates, dateStr, instance) {
                if (instance.input.getAttribute("autocompletePicker") == 1) {
                    instance.setDate(Date.now());
                }
            },
            locale: localeCode
        });
        $(prefixSelector + ".jjdecimal").each(applyDecimalPlaces);
        $(prefixSelector + "[data-toggle='tooltip'], " + prefixSelector + "[data-bs-toggle='tooltip']").tooltip({
            container: "body",
            trigger: "hover"
        });
    }
}
class CollapsePanelListener {
    static listen(name) {
        let nameSelector = "#" + name;
        let collapseSelector = '#collapse_mode_' + name;
        document.addEventListener("DOMContentLoaded", function () {
            let collapseElement = document.querySelector(nameSelector);
            collapseElement.addEventListener("hidden.bs.collapse", function () {
                document.querySelector(collapseSelector).value = "0";
            });
            collapseElement.addEventListener("show.bs.collapse", function () {
                document.querySelector(collapseSelector).value = "1";
            });
        });
    }
}
class DataDictionaryUtils {
    static deleteAction(actionName, url, questionStr) {
        let confirmed = confirm(questionStr);
        if (confirmed == true) {
            $.ajax({
                type: "POST",
                url: url,
                success: function (response) {
                    if (response.success) {
                        $("#" + actionName).remove();
                    }
                },
                error: function (xhr, status, error) {
                    SpinnerOverlay.hide();
                    if (xhr.responseText != "") {
                        var err = JSON.parse(xhr.responseText);
                        messageBox.show("JJMasterData", err.message, 4);
                    }
                    else {
                        console.log(xhr);
                    }
                }
            });
        }
    }
    static sortAction(context, url, errorStr) {
        $("#sortable-" + context).sortable({
            update: function () {
                var order = $(this).sortable('toArray');
                $.ajax({
                    type: "POST",
                    url: url,
                    data: { orderFields: order, context: context },
                    success: function (response) {
                        if (!response.success) {
                            messageBox.show("JJMasterData", errorStr, 4);
                        }
                    },
                    error: function (xhr, status, error) {
                        SpinnerOverlay.hide();
                        if (xhr.responseText != "") {
                            var err = JSON.parse(xhr.responseText);
                            if (err.status == 401) {
                                document.forms[0].submit();
                            }
                            else {
                                messageBox.show("JJMasterData", err.message, 4);
                            }
                        }
                        else {
                            messageBox.show("JJMasterData", errorStr, 4);
                        }
                    }
                });
            }
        }).disableSelection();
    }
    static setDisableAction(isDisable, url, errorStr) {
        $.ajax({
            type: "POST",
            url: url,
            data: { value: isDisable },
            success: function (response) {
                if (!response.success) {
                    messageBox.show("JJMasterData", errorStr, 4);
                }
            },
            error: function (xhr, status, error) {
                SpinnerOverlay.hide();
                if (xhr.responseText != "") {
                    var err = JSON.parse(xhr.responseText);
                    if (err.status == 401) {
                        document.forms[0].submit();
                    }
                    else {
                        messageBox.show("JJMasterData", err.message, 4);
                    }
                }
                else {
                    messageBox.show("JJMasterData", errorStr, 4);
                }
            }
        });
    }
    static refreshActionList() {
        SpinnerOverlay.show();
        window.parent.document.forms[0].requestSubmit();
    }
    static postAction(url) {
        SpinnerOverlay.show();
        $("form:first").attr("action", url).submit();
    }
    static exportElement(id, url, validStr) {
        var values = $("#grid-view-selected-rows-" + id).val();
        if (values == "") {
            messageBox.show("JJMasterData", validStr, 3);
            return false;
        }
        var form = $("form:first");
        var originAction = $("form:first").attr('action');
        form.attr('action', url);
        form.submit();
        setTimeout(function () {
            form.attr('action', originAction);
            SpinnerOverlay.hide();
        }, 2000);
        return true;
    }
}
class DataExportationHelper {
    static startProgressVerification(componentName, routeContext) {
        return __awaiter(this, void 0, void 0, function* () {
            DataExportationHelper.setLoadMessage();
            let urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("routeContext", routeContext);
            urlBuilder.addQueryParameter("gridViewName", componentName);
            urlBuilder.addQueryParameter("dataExportationOperation", "checkProgress");
            const url = urlBuilder.build();
            var isCompleted = false;
            while (!isCompleted) {
                isCompleted = yield DataExportationHelper.checkProgress(url, componentName);
                yield sleep(3000);
            }
        });
    }
    static stopExportation(componentName, routeContext, stopMessage) {
        return __awaiter(this, void 0, void 0, function* () {
            let urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("routeContext", routeContext);
            urlBuilder.addQueryParameter("gridViewName", componentName);
            urlBuilder.addQueryParameter("dataExportationOperation", "stopProcess");
            yield DataExportationHelper.stopProcess(urlBuilder.build(), stopMessage);
        });
    }
    static openExportPopup(componentName, routeContext) {
        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        urlBuilder.addQueryParameter("gridViewName", componentName);
        urlBuilder.addQueryParameter("dataExportationOperation", "showOptions");
        fetch(urlBuilder.build())
            .then(response => response.text())
            .then(data => {
            this.setSettingsHTML(componentName, data);
        })
            .catch(error => {
            console.log(error);
        });
    }
    static startExportation(componentName, routeContext) {
        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        urlBuilder.addQueryParameter("gridViewName", componentName);
        urlBuilder.addQueryParameter("dataExportationOperation", "startProcess");
        fetch(urlBuilder.build(), {
            method: "POST",
            body: new FormData(document.querySelector("form"))
        }).then(response => response.text()).then((html) => __awaiter(this, void 0, void 0, function* () {
            const modalBody = "#data-exportation-modal-" + componentName + " .modal-body ";
            document.querySelector(modalBody).innerHTML = html;
            listenAllEvents(modalBody);
            yield DataExportationHelper.startProgressVerification(componentName, routeContext);
        }));
    }
    static checkProgress(url, componentName) {
        return __awaiter(this, void 0, void 0, function* () {
            showSpinnerOnPost = false;
            try {
                const response = yield fetch(url);
                const data = yield response.json();
                if (data.FinishedMessage) {
                    showSpinnerOnPost = true;
                    document.querySelector("#data-exportation-modal-" + componentName + " .modal-body").innerHTML = data.FinishedMessage;
                    const linkFile = document.querySelector("#export_link_" + componentName);
                    if (linkFile)
                        linkFile.click();
                    return true;
                }
                else {
                    const processStatusElement = document.querySelector("#process-status");
                    const progressBarElement = document.querySelector(".progress-bar");
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
            }
            catch (e) {
                showSpinnerOnPost = true;
                const spinnerElement = document.querySelector("#data-exportation-spinner-" + componentName);
                if (spinnerElement) {
                    spinnerElement.style.display = "none";
                }
                document.querySelector("#data-exportation-modal-" + componentName + " .modal-body").innerHTML = e.message;
                return false;
            }
        });
    }
    static setLoadMessage() {
        const options = {
            lines: 13,
            length: 38,
            width: 17,
            radius: 45,
            scale: 0.2,
            corners: 1,
            color: "#000",
            opacity: 0.3,
            rotate: 0,
            direction: 1,
            speed: 1.2,
            trail: 62,
            fps: 20,
            zIndex: 2e9,
            className: "spinner",
            top: "50%",
            left: "50%",
            shadow: false,
            hwaccel: false,
            position: "absolute"
        };
        const target = document.getElementById('data-exportation-spinner-');
        var spinner = new Spinner(options).spin(target);
    }
    static setSettingsHTML(componentName, html) {
        const modalBody = document.querySelector("#data-exportation-modal-" + componentName + " .modal-body ");
        modalBody.innerHTML = html;
        const qtdElement = document.querySelector("#" + componentName + "_totrows");
        if (qtdElement) {
            const totRows = +qtdElement.textContent.replace(/\./g, "");
            if (totRows > 50000) {
                document.querySelector("#data-exportation-warning" + componentName).style.display = "block";
            }
        }
        if (bootstrapVersion < 5) {
            $("#data-exportation-modal-" + componentName).modal();
        }
        else {
            const modal = bootstrap.Modal.getOrCreateInstance(document.querySelector("#data-exportation-modal-" + componentName), {});
            modal.show();
        }
        listenAllEvents("#data-exportation-modal-" + componentName);
    }
    static stopProcess(url, stopMessage) {
        return __awaiter(this, void 0, void 0, function* () {
            document.querySelector("#process-status").innerHTML = stopMessage;
            showSpinnerOnPost = false;
            yield fetch(url);
        });
    }
    static showOptions(componentName, exportType) {
        const orientationDiv = document.getElementById(`${componentName}-div-export-orientation`);
        const allDiv = document.getElementById(`${componentName}-div-export-all`);
        const delimiterDiv = document.getElementById(`${componentName}-div-export-delimiter`);
        const firstlineDiv = document.getElementById(`${componentName}-div-export-firstline`);
        if (exportType === "1") {
            if (orientationDiv)
                orientationDiv.style.display = "none";
            if (allDiv)
                allDiv.style.display = "block";
            if (delimiterDiv)
                delimiterDiv.style.display = "none";
            if (firstlineDiv)
                firstlineDiv.style.display = "block";
        }
        else if (exportType === "2") {
            if (orientationDiv)
                orientationDiv.style.display = "block";
            if (allDiv)
                allDiv.style.display = "none";
            if (delimiterDiv)
                delimiterDiv.style.display = "none";
            if (firstlineDiv)
                firstlineDiv.style.display = "none";
        }
        else {
            if (orientationDiv)
                orientationDiv.style.display = "none";
            if (allDiv)
                allDiv.style.display = "block";
            if (delimiterDiv)
                delimiterDiv.style.display = "block";
            if (firstlineDiv)
                firstlineDiv.style.display = "block";
        }
    }
}
class DataImportationHelper {
    static setLoadMessage() {
        const options = {
            lines: 13,
            length: 38,
            width: 17,
            radius: 45,
            scale: 0.2,
            corners: 1,
            color: "#000",
            opacity: 0.3,
            rotate: 0,
            direction: 1,
            speed: 1.2,
            trail: 62,
            fps: 20,
            zIndex: 2e9,
            className: "spinner",
            top: "50%",
            left: "50%",
            shadow: false,
            hwaccel: false,
            position: "absolute"
        };
        const target = document.getElementById('impSpin');
        new Spinner(options).spin(target);
    }
    static checkProgress(componentName, importationRouteContext, gridRouteContext) {
        showSpinnerOnPost = false;
        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", importationRouteContext);
        urlBuilder.addQueryParameter("dataImportationOperation", "checkProgress");
        urlBuilder.addQueryParameter("componentName", componentName);
        const url = urlBuilder.build();
        fetch(url, {
            method: 'GET',
            cache: 'no-cache',
            headers: {
                'Content-Type': 'application/json',
            },
        })
            .then(response => response.json())
            .then(result => {
            const processMessageDiv = document.querySelector("#process-status");
            if (processMessageDiv) {
                processMessageDiv.style.display = "";
            }
            const progressBar = document.querySelector(".progress-bar");
            if (progressBar) {
                progressBar.style.width = result.PercentProcess + "%";
                progressBar.textContent = result.PercentProcess + "%";
            }
            const processMessage = document.querySelector("#process-message");
            if (processMessage) {
                processMessage.textContent = result.Message;
            }
            const startDateLabel = document.querySelector("#start-date-label");
            if (startDateLabel) {
                startDateLabel.textContent = result.StartDate;
            }
            if (result.Insert > 0) {
                document.querySelector("#lblInsert").style.display = "";
                if (result.PercentProcess === 100) {
                    document.querySelector("#lblInsertCount").textContent = result.Insert;
                }
                else {
                    jjutil.animateValue("lblInsertCount", DataImportationHelper.insertCount, result.Insert, 1000);
                }
                DataImportationHelper.insertCount = result.Insert;
            }
            if (result.Update > 0) {
                document.querySelector("#lblUpdate").style.display = "";
                if (result.PercentProcess === 100) {
                    document.querySelector("#lblUpdateCount").textContent = result.Update;
                }
                else {
                    jjutil.animateValue("lblUpdateCount", DataImportationHelper.updateCount, result.Update, 1000);
                }
                DataImportationHelper.updateCount = result.Update;
            }
            if (result.Delete > 0) {
                document.querySelector("#lblDelete").style.display = "";
                if (result.PercentProcess === 100) {
                    document.querySelector("#lblDeleteCount").textContent = result.Delete;
                }
                else {
                    jjutil.animateValue("lblDeleteCount", DataImportationHelper.deleteCount, result.Delete, 1000);
                }
                DataImportationHelper.deleteCount = result.Delete;
            }
            if (result.Ignore > 0) {
                document.querySelector("#lblIgnore").style.display = "";
                if (result.PercentProcess === 100) {
                    document.querySelector("#lblIgnoreCount").textContent = result.Ignore;
                }
                else {
                    jjutil.animateValue("lblIgnoreCount", DataImportationHelper.ignoreCount, result.Ignore, 1000);
                }
                DataImportationHelper.ignoreCount = result.Ignore;
            }
            if (result.Error > 0) {
                document.querySelector("#lblError").style.display = "";
                if (result.PercentProcess === 100) {
                    document.querySelector("#lblErrorCount").textContent = result.Error;
                }
                else {
                    jjutil.animateValue("lblErrorCount", DataImportationHelper.errorCount, result.Error, 1000);
                }
                DataImportationHelper.errorCount = result.Error;
            }
            if (!result.IsProcessing) {
                clearInterval(DataImportationHelper.intervalId);
                let urlBuilder = new UrlBuilder();
                urlBuilder.addQueryParameter("routeContext", importationRouteContext);
                urlBuilder.addQueryParameter("dataImportationOperation", "log");
                DataImportationModal.getInstance().showUrl({ url: urlBuilder.build() }, "Import", ModalSize.ExtraLarge).then(_ => {
                    GridViewHelper.refreshGrid(componentName, gridRouteContext);
                });
            }
        })
            .catch(error => {
            console.error('Error fetching data:', error);
        });
    }
    static show(componentName, routeContext, gridRouteContext) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        DataImportationHelper.addPasteListener(componentName, routeContext, gridRouteContext);
        DataImportationModal.getInstance().showUrl({
            url: urlBuilder.build(),
            requestOptions: { method: "POST", body: new FormData(document.querySelector("form")) }
        }, "Import", ModalSize.ExtraLarge).then(_ => {
            UploadAreaListener.listenFileUpload();
        });
    }
    static showLog(componentName, routeContext) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        urlBuilder.addQueryParameter("dataImportationOperation", "log");
        DataImportationModal.getInstance().showUrl({ url: urlBuilder.build() }, "Import", ModalSize.ExtraLarge);
    }
    static start(componentName, routeContext, gridRouteContext) {
        DataImportationHelper.setLoadMessage();
        DataImportationHelper.intervalId = setInterval(function () {
            DataImportationHelper.checkProgress(componentName, routeContext, gridRouteContext);
        }, 3000);
    }
    static help(componentName, routeContext) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        urlBuilder.addQueryParameter("dataImportationOperation", "help");
        postFormValues({
            url: urlBuilder.build(), success: html => {
                document.querySelector("#" + componentName).innerHTML = html;
            }
        });
    }
    static stop(componentName, routeContext, stopLabel) {
        showSpinnerOnPost = false;
        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        urlBuilder.addQueryParameter("dataImportationOperation", "checkProgress");
        urlBuilder.addQueryParameter("componentName", componentName);
        const url = urlBuilder.build();
        fetch(url).then(response => response.json()).then(data => {
            if (data.isProcessing === false) {
                document.getElementById("divMsgProcess").innerHTML = stopLabel;
            }
        });
    }
    static addPasteListener(componentName, routeContext, gridRouteContext) {
        DataImportationHelper.pasteEventListener = function onPaste(e) {
            DataImportationHelper.removePasteListener();
            let pastedText = undefined;
            if (window.clipboardData && window.clipboardData.getData) {
                pastedText = window.clipboardData.getData("Text");
            }
            else if (e.clipboardData && e.clipboardData.getData) {
                pastedText = e.clipboardData.getData("text/plain");
            }
            e.preventDefault();
            if (pastedText != undefined) {
                document.querySelector("#pasteValue").value = pastedText;
                let urlBuilder = new UrlBuilder();
                urlBuilder.addQueryParameter("routeContext", routeContext);
                urlBuilder.addQueryParameter("dataImportationOperation", "processPastedText");
                DataImportationModal.getInstance().showUrl({
                    url: urlBuilder.build(),
                    requestOptions: { method: "POST", body: new FormData(document.querySelector("form")) }
                }, "Import", ModalSize.Small).then(_ => {
                    DataImportationHelper.start(componentName, routeContext, gridRouteContext);
                });
            }
            return false;
        };
        document.addEventListener("paste", DataImportationHelper.pasteEventListener, { once: true });
    }
    static removePasteListener() {
        if (DataImportationHelper.pasteEventListener) {
            document.removeEventListener("paste", DataImportationHelper.pasteEventListener);
        }
    }
}
DataImportationHelper.insertCount = 0;
DataImportationHelper.updateCount = 0;
DataImportationHelper.deleteCount = 0;
DataImportationHelper.ignoreCount = 0;
DataImportationHelper.errorCount = 0;
class DataImportationModal {
    static getInstance() {
        if (this.instance === undefined) {
            this.instance = new Modal();
            this.instance.onModalHidden = DataImportationHelper.removePasteListener;
        }
        return this.instance;
    }
}
class DataPanelHelper {
    static reload(componentName, fieldName, routeContext) {
        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("panelName", componentName);
        urlBuilder.addQueryParameter("componentName", fieldName);
        urlBuilder.addQueryParameter("routeContext", routeContext);
        const form = document.querySelector("form");
        fetch(urlBuilder.build(), {
            method: form.method,
            body: new FormData(form),
        })
            .then(response => {
            if (!response.ok) {
                throw new Error(response.statusText);
            }
            return response.text();
        })
            .then(data => {
            document.getElementById(componentName).outerHTML = data;
            listenAllEvents();
            jjutil.gotoNextFocus(fieldName);
        })
            .catch(error => {
            console.error(error);
        });
    }
}
function applyDecimalPlaces() {
    let decimalPlaces = $(this).attr("jjdecimalplaces");
    if (decimalPlaces == null)
        decimalPlaces = "2";
    if (localeCode === 'pt')
        $(this).number(true, decimalPlaces, ",", ".");
    else
        $(this).number(true, decimalPlaces);
}
class FeedbackIcon {
    static removeAllIcons(selector) {
        const elements = document.querySelectorAll(selector);
        elements === null || elements === void 0 ? void 0 : elements.forEach(element => {
            element.classList.remove(FeedbackIcon.successClass, FeedbackIcon.warningClass, FeedbackIcon.searchClass, FeedbackIcon.errorClass);
        });
    }
    static setIcon(selector, iconClass) {
        this.removeAllIcons(selector);
        const elements = document.querySelectorAll(selector);
        elements === null || elements === void 0 ? void 0 : elements.forEach(element => {
            element.classList.add(iconClass);
        });
    }
}
FeedbackIcon.searchClass = "jj-icon-search";
FeedbackIcon.successClass = "jj-icon-success";
FeedbackIcon.warningClass = "jj-icon-warning";
FeedbackIcon.errorClass = "jj-icon-error";
class FormViewHelper {
    static showInsertSuccess(componentName) {
        const insertMessagePanel = document.getElementById("insert-message-panel" + componentName);
        const insertPanel = document.getElementById("insert-panel" + componentName);
        if (insertMessagePanel && insertPanel) {
            insertMessagePanel.style.transition = "opacity 2s ease";
            insertMessagePanel.style.opacity = "0";
            setTimeout(() => {
                insertMessagePanel.style.display = "none";
                insertPanel.style.display = "block";
            }, 2000);
        }
    }
    static openSelectElementInsert(componentName, encryptedActionMap) {
        const currentActionInput = document.querySelector(`#form-view-current-action-${componentName}`);
        const selectActionValuesInput = document.querySelector(`#form-view-select-action-values${componentName}`);
        const form = document.querySelector('form');
        if (currentActionInput && selectActionValuesInput && form) {
            currentActionInput.value = 'ELEMENTSEL';
            selectActionValuesInput.value = encryptedActionMap;
            form.requestSubmit();
        }
    }
}
var _a, _b;
var showSpinnerOnPost = true;
var bootstrapVersion = (() => {
    const htmlElement = document.querySelector('html');
    const versionAttribute = htmlElement === null || htmlElement === void 0 ? void 0 : htmlElement.getAttribute('data-bs-version');
    if (versionAttribute) {
        return parseInt(versionAttribute, 10);
    }
    return 5;
})();
const locale = (_a = document.documentElement.lang) !== null && _a !== void 0 ? _a : 'pt-BR';
const localeCode = (_b = locale.split("-")[0]) !== null && _b !== void 0 ? _b : 'pt';
class GridViewFilterHelper {
    static filter(componentName, routeContext) {
        document.querySelector("#grid-view-filter-action-" + componentName).value = "FILTERACTION";
        document.querySelector("#grid-view-action-" + componentName).value = "";
        document.querySelector("#grid-view-page-" + componentName).value = "1";
        GridViewHelper.clearCurrentFormAction(componentName);
        GridViewHelper.refreshGrid(componentName, routeContext);
    }
    static clearFilterInputs(componentName) {
        const divId = "#current-grid-filter-" + componentName;
        const selector = divId + " input:enabled, " + divId + " select:enabled";
        $(selector).each(function () {
            let currentObj = $(this);
            if (currentObj.hasClass("flatpickr-input")) {
                currentObj.val("");
            }
            let inputType = this.type;
            if (inputType == "checkbox") {
                currentObj.prop("checked", false);
            }
            else if (inputType != "input" && currentObj.attr("data-role") == "tagsinput") {
                currentObj.tagsinput('removeAll');
            }
            else if (inputType != "hidden") {
                currentObj.val("");
                if (currentObj.hasClass("selectpicker")) {
                    currentObj.selectpicker("render");
                }
                else if (currentObj.hasClass("jj-search-box")) {
                    currentObj.blur();
                }
                else if (currentObj.hasClass("jjlookup")) {
                    currentObj.blur();
                }
            }
        });
        document.querySelector("#grid-view-filter-action-" + componentName).value = "CLEARACTION";
        document.querySelector("#grid-view-action-" + componentName).value = "";
        GridViewHelper.clearCurrentFormAction(componentName);
    }
    static clearFilter(componentName, routeContext) {
        this.clearFilterInputs(componentName);
        GridViewHelper.refreshGrid(componentName, routeContext);
    }
    static searchOnDOM(objid, oDom) {
        var value = $(oDom).val().toString().toLowerCase();
        $("#table_" + objid + " tr").filter(function () {
            var textValues = $(this).clone().find('.bootstrap-select, .selectpicker, select').remove().end().text();
            var isSearch = textValues.toLowerCase().indexOf(value) > -1;
            if (!isSearch) {
                var valueNew = value.replace(",", "").replace(".", "").replace("-", "");
                $(this).find("input").each(function () {
                    var inputValue = $(this).val();
                    if (inputValue != null) {
                        let isSearch = inputValue.toString().replace(",", "")
                            .replace(".", "")
                            .replace("-", "")
                            .toLowerCase()
                            .indexOf(valueNew) > -1;
                        if (isSearch)
                            return false;
                    }
                });
            }
            if (!isSearch) {
                $(this).find("select").each(function () {
                    var selectedText = $(this).children("option:selected").text();
                    if (selectedText != null) {
                        isSearch = selectedText.toLowerCase().indexOf(valueNew) > -1;
                        if (isSearch)
                            return false;
                    }
                });
            }
            $(this).toggle(isSearch);
        });
        if (value.length > 0) {
            $("#infotext_" + objid).css("display", "none");
            $("ul.pagination").css("display", "none");
        }
        else {
            $("#infotext_" + objid).css("display", "");
            $("ul.pagination").css("display", "");
        }
    }
}
class GridViewHelper {
    static openSettingsModal(componentName, encryptedActionMap) {
        const gridViewActionInput = document.getElementById("grid-view-action-" + componentName);
        const gridViewPageInput = document.getElementById("grid-view-page-" + componentName);
        const gridViewRowInput = document.getElementById("grid-view-row-" + componentName);
        const form = document.querySelector("form");
        if (gridViewActionInput && gridViewPageInput && gridViewRowInput && form) {
            gridViewActionInput.value = encryptedActionMap;
            gridViewPageInput.value = "1";
            gridViewRowInput.value = "";
            this.clearCurrentFormAction(componentName);
            form.requestSubmit();
        }
    }
    static closeSettingsModal(componentName) {
        const form = document.querySelector("form");
        const checkboxes = document.querySelectorAll("form");
        const modal = document.getElementById("config-modal-" + componentName);
        if (form) {
            form.reset();
        }
        if (checkboxes) {
            checkboxes.forEach((checkbox) => {
                if (checkbox instanceof HTMLInputElement) {
                    checkbox.dispatchEvent(new Event("change", { bubbles: true, cancelable: true }));
                }
            });
        }
        if (modal) {
            modal.classList.remove("show");
            modal.style.display = "none";
        }
    }
    static sortGridValues(componentName, routeContext, field) {
        const tableOrderElement = document.querySelector("#grid-view-order-" + componentName);
        if (field + " ASC" === tableOrderElement.value)
            tableOrderElement.value = field + " DESC";
        else
            tableOrderElement.value = field + " ASC";
        document.querySelector("#grid-view-action-" + componentName).value = "";
        this.clearCurrentFormAction(componentName);
        GridViewHelper.refreshGrid(componentName, routeContext);
    }
    static sortItems(componentName) {
        var descCommand = "";
        var order = $("#sortable-" + componentName).sortable("toArray");
        for (var i = 0; i < order.length; i++) {
            var sortingType = $("#" + order[i] + "_order").children("option:selected").val();
            switch (sortingType) {
                case "A":
                    descCommand += order[i] + " ASC,";
                    break;
                case "D":
                    descCommand += order[i] + " DESC,";
                    break;
            }
        }
        descCommand = descCommand.substring(0, descCommand.length - 1);
        document.querySelector("#grid-view-order-" + componentName).value = descCommand;
        $("#" + componentName + "-sort-modal").modal('hide');
        this.clearCurrentFormAction(componentName);
    }
    static clearCurrentFormAction(componentName) {
        const currentFormAction = document.querySelector("#form-view-action-map-" + componentName);
        if (currentFormAction)
            currentFormAction.value = "";
    }
    static setCurrentGridPage(componentName, currentPage) {
        const currentGridPage = document.querySelector("#grid-view-page-" + componentName);
        if (currentGridPage)
            currentGridPage.value = currentPage;
    }
    static clearCurrentGridAction(componentName) {
        const currentGridAction = document.querySelector("#grid-view-action-" + componentName);
        if (currentGridAction)
            currentGridAction.value = "";
    }
    static paginate(componentName, routeContext, currentPage) {
        this.setCurrentGridPage(componentName, currentPage);
        this.clearCurrentGridAction(componentName);
        this.clearCurrentFormAction(componentName);
        GridViewHelper.refreshGrid(componentName, routeContext);
    }
    static refresh(componentName, routeContext) {
        this.setCurrentGridPage(componentName, String());
        this.clearCurrentGridAction(componentName);
        this.clearCurrentFormAction(componentName);
        GridViewHelper.refreshGrid(componentName, routeContext);
    }
    static refreshGrid(componentName, routeContext, reloadListeners = false) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        postFormValues({
            url: urlBuilder.build(),
            success: function (data) {
                const gridViewElement = document.querySelector("#grid-view-" + componentName);
                const filterActionElement = document.querySelector("#grid-view-filter-action-" + componentName);
                if (gridViewElement) {
                    gridViewElement.innerHTML = data;
                    if (reloadListeners) {
                        listenAllEvents();
                    }
                    if (filterActionElement) {
                        filterActionElement.value = "";
                    }
                }
                else {
                    console.error("One or both of the elements were not found.");
                }
            },
            error: function (error) {
                console.error(error);
                const filterActionElement = document.querySelector("#grid-view-filter-action-" + componentName);
                if (filterActionElement) {
                    filterActionElement.value = "";
                }
                else {
                    console.error("Filter action element was not found.");
                }
            }
        });
    }
}
class GridViewSelectionHelper {
    static selectItem(componentName, obj) {
        const valuesInput = document.getElementById("grid-view-selected-rows-" + componentName);
        const values = valuesInput.value.toString();
        let valuesList = [];
        if (obj.id === "jjcheckbox-select-all-rows") {
            return;
        }
        if (values.length > 0) {
            valuesList = values.split(",");
        }
        if (obj.checked) {
            if (valuesList.indexOf(obj.value) < 0) {
                valuesList.push(obj.value);
            }
        }
        else {
            valuesList = valuesList.filter((item) => item !== obj.value);
        }
        valuesInput.value = valuesList.join(",");
        let textInfo = "";
        const selectedText = document.getElementById("selected-text-" + componentName);
        if (valuesList.length === 0) {
            textInfo = (selectedText === null || selectedText === void 0 ? void 0 : selectedText.getAttribute("no-record-selected-label")) || "";
        }
        else if (valuesList.length === 1) {
            textInfo = (selectedText === null || selectedText === void 0 ? void 0 : selectedText.getAttribute("one-record-selected-label")) || "";
        }
        else {
            const multipleRecordsLabel = (selectedText === null || selectedText === void 0 ? void 0 : selectedText.getAttribute("multiple-records-selected-label")) || "";
            textInfo = multipleRecordsLabel.replace("{0}", valuesList.length.toString());
        }
        if (selectedText) {
            selectedText.textContent = textInfo;
        }
    }
    static selectAll(componentName, routeContext) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        postFormValues({
            url: urlBuilder.build(),
            success: (data) => {
                this.selectAllRowsElements(componentName, data.selectedRows);
            }
        });
    }
    static selectAllRowsElements(componentName, rows) {
        const values = rows.split(",");
        const checkboxes = document.querySelectorAll(".jjselect input:not(:disabled)");
        checkboxes.forEach(checkbox => checkbox.checked = true);
        const selectedRowsInput = document.getElementById("grid-view-selected-rows-" + componentName);
        selectedRowsInput.value = values.join(",");
        const selectedText = document.getElementById("selected-text-" + componentName);
        selectedText.textContent = selectedText.getAttribute("multiple-records-selected-label").replace("{0}", values.length.toString());
    }
    static unSelectAll(componentName) {
        const checkboxes = document.querySelectorAll(`#${componentName} .jjselect input:not(:disabled)`);
        const valuesInput = document.getElementById("grid-view-selected-rows-" + componentName);
        const selectedText = document.getElementById("selected-text-" + componentName);
        if (checkboxes) {
            checkboxes.forEach((checkbox) => {
                checkbox.checked = false;
            });
        }
        if (valuesInput) {
            valuesInput.value = "";
        }
        if (selectedText) {
            selectedText.textContent = selectedText.getAttribute("no-record-selected-label") || "";
        }
    }
}
document.addEventListener("DOMContentLoaded", function () {
    listenAllEvents();
});
const listenAllEvents = (selectorPrefix = String()) => {
    selectorPrefix += " ";
    $(selectorPrefix + ".selectpicker").selectpicker({
        iconBase: 'fa'
    });
    $(selectorPrefix + "input[type=checkbox][data-toggle^=toggle]").bootstrapToggle();
    CalendarListener.listen(selectorPrefix);
    TextAreaListener.listenKeydown(selectorPrefix);
    SearchBoxListener.listenTypeahed(selectorPrefix);
    LookupListener.listenChanges(selectorPrefix);
    SortableListener.listenSorting(selectorPrefix);
    UploadAreaListener.listenFileUpload(selectorPrefix);
    TabNavListener.listenTabNavs(selectorPrefix);
    SliderListener.listenSliders(selectorPrefix);
    SliderListener.listenInputs(selectorPrefix);
    $(document).on({
        ajaxSend: function (event, jqXHR, settings) {
            if (settings.url != null &&
                settings.url.indexOf("context=searchBox") !== -1) {
                return null;
            }
            if (showSpinnerOnPost) {
                SpinnerOverlay.show();
            }
        },
        ajaxStop: function () { SpinnerOverlay.hide(); }
    });
    document.querySelector("form").addEventListener("submit", function (event) {
        let isValid;
        if (typeof this.reportValidity === "function") {
            isValid = this.reportValidity();
        }
        else {
            isValid = true;
        }
        if (isValid && showSpinnerOnPost) {
            setTimeout(function () {
                SpinnerOverlay.show();
            }, 1);
        }
    });
};
class LookupHelper {
    static setLookupValues(fieldName, id, description) {
        defaultModal.hide();
        const idInput = document.querySelector("#" + fieldName);
        idInput.value = id;
        const descriptionInput = document.querySelector("#" + fieldName + "-description");
        if (descriptionInput) {
            descriptionInput.value = description;
        }
    }
}
class LookupListener {
    static listenChanges(selectorPrefix = String()) {
        const lookupInputs = document.querySelectorAll(selectorPrefix + "input.jj-lookup");
        lookupInputs.forEach(lookupInput => {
            let lookupId = lookupInput.id;
            let lookupDescriptionUrl = lookupInput.getAttribute("lookup-description-url");
            const lookupIdSelector = "#" + lookupId;
            const lookupDescriptionSelector = lookupIdSelector + "-description";
            lookupInput.addEventListener("blur", function () {
                FeedbackIcon.removeAllIcons(lookupDescriptionSelector);
                postFormValues({
                    url: lookupDescriptionUrl,
                    success: (data) => {
                        if (data.description === "") {
                            FeedbackIcon.setIcon(lookupIdSelector, FeedbackIcon.warningClass);
                        }
                        else {
                            const lookupIdInput = document.querySelector(lookupIdSelector);
                            const lookupDescriptionInput = document.querySelector(lookupDescriptionSelector);
                            FeedbackIcon.setIcon(lookupIdSelector, FeedbackIcon.successClass);
                            lookupIdInput.value = data.id;
                            if (lookupDescriptionInput) {
                                lookupDescriptionInput.value = data.description;
                            }
                        }
                    },
                    error: (_) => {
                        FeedbackIcon.setIcon(lookupIdSelector, FeedbackIcon.errorClass);
                    }
                });
            });
        });
    }
}
var TMessageIcon;
(function (TMessageIcon) {
    TMessageIcon[TMessageIcon["NONE"] = 1] = "NONE";
    TMessageIcon[TMessageIcon["INFO"] = 2] = "INFO";
    TMessageIcon[TMessageIcon["WARNING"] = 3] = "WARNING";
    TMessageIcon[TMessageIcon["ERROR"] = 4] = "ERROR";
    TMessageIcon[TMessageIcon["QUESTION"] = 5] = "QUESTION";
})(TMessageIcon || (TMessageIcon = {}));
var TMessageSize;
(function (TMessageSize) {
    TMessageSize[TMessageSize["SMALL"] = 1] = "SMALL";
    TMessageSize[TMessageSize["DEFAULT"] = 2] = "DEFAULT";
    TMessageSize[TMessageSize["LARGE"] = 3] = "LARGE";
})(TMessageSize || (TMessageSize = {}));
class MessageBox {
    static setTitle(title) {
        $(MessageBox.jQueryModalTitleId).html(title);
    }
    static setContent(content) {
        $(MessageBox.jQueryModalContentId).html(content);
    }
    static showModal() {
        if (MessageBox.bootstrapVersion < 5) {
            $(MessageBox.jQueryModalId)
                .modal()
                .on("shown.bs.modal", function () {
                $(MessageBox.jQueryModalButton1Id).focus();
            });
        }
        else {
            const modal = new bootstrap.Modal(document.getElementById(MessageBox.modalId), {});
            modal.show();
            modal.addEventListener("shown.bs.modal", function () {
                document.getElementById(MessageBox.button1Id).focus();
            });
        }
    }
    static setBtn1(label, func) {
        $(MessageBox.jQueryModalButton1Id).text(label);
        if ($.isFunction(func)) {
            $(MessageBox.jQueryModalButton1Id).on("click.siteModalClick1", func);
        }
        $(MessageBox.jQueryModalButton1Id).show();
    }
    static setBtn2(label, func) {
        $(MessageBox.jQueryModalButton2Id).text(label);
        if ($.isFunction(func)) {
            $(MessageBox.jQueryModalButton2Id).on("click.siteModalClick2", func);
        }
        $(MessageBox.jQueryModalButton2Id).show();
    }
    static reset() {
        MessageBox.setTitle("");
        MessageBox.setContent("");
        $(MessageBox.jQueryModalButton1Id).text("");
        $(MessageBox.jQueryModalButton1Id).off("click.siteModalClick1");
        $(MessageBox.jQueryModalButton2Id).text("");
        $(MessageBox.jQueryModalButton2Id).off("click.siteModalClick2");
    }
    static loadHtml(icontype, sizetype) {
        if ($(MessageBox.jQueryModalId).length) {
            $(MessageBox.jQueryModalId).remove();
        }
        let html = "";
        html += "<div id=\"site-modal\" tabindex=\"-1\" class=\"modal fade\" role=\"dialog\">\r\n";
        html += "  <div class=\"modal-dialog";
        if (sizetype == TMessageSize.LARGE)
            html += " modal-lg";
        else if (sizetype == TMessageSize.SMALL)
            html += " modal-sm";
        html += "\" role=\"document\">\r\n";
        html += "    <div class=\"modal-content\">\r\n";
        html += "      <div class=\"modal-header\">\r\n";
        if (MessageBox.bootstrapVersion >= 4) {
            html += "        <h4 id=\"site-modal-title\" class=\"modal-title\"></h4>\r\n";
        }
        else if (MessageBox.bootstrapVersion >= 5) {
            html +=
                '        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>\r\n';
        }
        else if (MessageBox.bootstrapVersion == 3) {
            html +=
                '        <h4 id="site-modal-title" class="modal-title"><button type="button" class="close" data-dismiss="modal">&times;</button></h4>\r\n';
        }
        html += "      </div>\r\n";
        html += "      <div class=\"modal-body\">\r\n";
        html += "        <table border=\"0\">\r\n";
        html += "          <tr>\r\n";
        html += '            <td style="width:40px">\r\n';
        if (icontype == TMessageIcon.ERROR) {
            html += '              <span class="text-danger">\r\n';
            html +=
                '                <span class="fa fa-times-circle" aria-hidden="true" style="font-size: 30px;"></span>\r\n';
            html += "              </span>\r\n";
        }
        else if (icontype == TMessageIcon.WARNING) {
            html += '              <span class="text-warning">\r\n';
            html +=
                '                <span class="fa fa-exclamation-triangle " aria-hidden="true" style="font-size: 30px;"></span>\r\n';
            html += "              </span>\r\n";
        }
        else if (icontype == TMessageIcon.INFO) {
            html += '              <span class="text-info">\r\n';
            html +=
                '                <span class="fa fa-info-circle" aria-hidden="true" style="font-size: 30px;"></span>\r\n';
            html += "              </span>\r\n";
        }
        else if (icontype == TMessageIcon.QUESTION) {
            html += '              <span class="text-info">\r\n';
            html +=
                '                <span class="fa fa-question-circle" aria-hidden="true" style="font-size: 30px;"></span>\r\n';
            html += "              </span>\r\n";
        }
        html += "            </td>\r\n";
        html += "            <td>\r\n";
        html += '              <span id="site-modal-content"></span>\r\n';
        html += "            </td>\r\n";
        html += "          </tr>\r\n";
        html += "        </table>\r\n";
        html += "      </div>\r\n";
        html += "      <div class=\"modal-footer\">\r\n";
        if (MessageBox.bootstrapVersion == 3) {
            html += '        <button type="button" id="site-modal-btn1" class="btn btn-default" data-dismiss="modal"></button>\r\n';
            html += '        <button type="button" id="site-modal-btn2" class="btn btn-default" data-dismiss="modal"></button>\r\n';
        }
        else if (MessageBox.bootstrapVersion == 4) {
            html += '        <button type="button" id="site-modal-btn1" class="btn btn-outline-dark" data-dismiss="modal"></button>\r\n';
            html += '        <button type="button" id="site-modal-btn2" class="btn btn-outline-dark" data-dismiss="modal"></button>\r\n';
        }
        else {
            html += '        <button type="button" id="site-modal-btn1" class="btn btn-outline-dark" data-bs-dismiss="modal"></button>\r\n';
            html += '        <button type="button" id="site-modal-btn2" class="btn btn-outline-dark" data-bs-dismiss="modal"></button>\r\n';
        }
        html += "      </div>\r\n";
        html += "    </div>\r\n";
        html += "  </div>\r\n";
        html += "</div>\r\n";
        $("body").append(html);
    }
    static show(title, content, icontype, sizetype, btn1Label, btn1Func, btn2Label, btn2Func) {
        MessageBox.reset();
        MessageBox.loadHtml(icontype, sizetype || TMessageSize.DEFAULT);
        MessageBox.setTitle(title);
        MessageBox.setContent(content);
        if (btn1Label === undefined) {
            MessageBox.setBtn1("Fechar", null);
        }
        else {
            MessageBox.setBtn1(btn1Label, btn1Func);
        }
        if (btn2Label === undefined) {
            $(MessageBox.jQueryModalButton2Id).hide();
        }
        else {
            MessageBox.setBtn2(btn2Label, btn2Func);
        }
        MessageBox.showModal();
    }
    static hide() {
        $(MessageBox.jQueryModalId).modal("hide");
        $(".modal-backdrop").hide();
    }
}
MessageBox.jQueryModalId = "#site-modal";
MessageBox.jQueryModalTitleId = "#site-modal-title";
MessageBox.jQueryModalContentId = "#site-modal-content";
MessageBox.jQueryModalButton1Id = "#site-modal-btn1";
MessageBox.jQueryModalButton2Id = "#site-modal-btn2";
MessageBox.modalId = MessageBox.jQueryModalId.substring(1);
MessageBox.button1Id = MessageBox.jQueryModalButton1Id.substring(1);
MessageBox.bootstrapVersion = 5;
const messageBox = MessageBox;
var ModalSize;
(function (ModalSize) {
    ModalSize[ModalSize["Default"] = 0] = "Default";
    ModalSize[ModalSize["ExtraLarge"] = 1] = "ExtraLarge";
    ModalSize[ModalSize["Large"] = 2] = "Large";
    ModalSize[ModalSize["Small"] = 3] = "Small";
    ModalSize[ModalSize["Fullscreen"] = 4] = "Fullscreen";
})(ModalSize || (ModalSize = {}));
class ModalUrlOptions {
}
class ModalBase {
    constructor() {
        this.modalId = "jjmasterdata-modal";
        this.modalSize = ModalSize.ExtraLarge;
    }
}
class _Modal extends ModalBase {
    constructor() {
        super(...arguments);
        this.modalSizeCssClass = {
            Default: "jj-modal-default",
            ExtraLarge: "jj-modal-xl",
            Large: "jj-modal-lg",
            Small: "jj-modal-sm",
            Fullscreen: "modal-fullscreen",
        };
    }
    getBootstrapModal() {
        return bootstrap.Modal.getOrCreateInstance("#" + this.modalId);
    }
    showModal() {
        this.getBootstrapModal().show();
    }
    hideModal() {
        this.getBootstrapModal().hide();
    }
    getModalCssClass() {
        return this.modalSizeCssClass[ModalSize[this.modalSize]];
    }
    createModalElement() {
        if (!document.getElementById(this.modalId)) {
            this.modalElement = document.createElement("div");
            this.modalElement.id = this.modalId;
            this.modalElement.classList.add("modal", "fade");
            this.modalElement.tabIndex = -1;
            this.modalElement.setAttribute("role", "dialog");
            this.modalElement.setAttribute("aria-labelledby", `${this.modalId}-label`);
            this.modalElement.innerHTML = `
      <div id="${this.modalId}-dialog" class="modal-dialog ${this.centered ? "modal-dialog-centered" : ""} modal-dialog-scrollable ${this.getModalCssClass()}" role="document">
        <div class="modal-content" >
          <div class="modal-header">
            <h5 class="modal-title" id="${this.modalId}-label">${this.modalTitle}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body"> </div>
        </div>
      </div>`;
            let form = document.forms[0];
            if (form) {
                form.appendChild(this.modalElement);
            }
            else {
                document.body.appendChild(this.modalElement);
            }
            const onModalHidden = this.onModalHidden;
            this.modalElement.addEventListener('hidden.bs.modal', () => {
                if (onModalHidden) {
                    onModalHidden();
                }
            });
        }
        else {
            this.modalElement = document.getElementById(this.modalId);
            const dialog = document.getElementById(this.modalId + "-dialog");
            Object.values(ModalSize).forEach(cssClass => {
                dialog.classList.remove(cssClass);
            });
            dialog.classList.add(this.getModalCssClass());
        }
    }
    showIframe(url, title, size = null) {
        this.modalTitle = title;
        this.modalSize = size !== null && size !== void 0 ? size : ModalSize.Default;
        this.createModalElement();
        const modalBody = this.modalElement.querySelector(".modal-body");
        let style = "width: 100vw; height: 100vh;";
        modalBody.innerHTML = `<iframe src="${url}" frameborder="0" style="${style}"></iframe>`;
        this.showModal();
    }
    showUrl(options, title, size = null) {
        return __awaiter(this, void 0, void 0, function* () {
            this.modalTitle = title;
            this.modalSize = size !== null && size !== void 0 ? size : ModalSize.Default;
            this.createModalElement();
            let fetchUrl;
            let fetchOptions;
            if (options instanceof ModalUrlOptions) {
                fetchUrl = options.url;
                fetchOptions = options.requestOptions;
            }
            else {
                fetchUrl = options;
            }
            return yield fetch(fetchUrl, fetchOptions)
                .then((response) => __awaiter(this, void 0, void 0, function* () {
                var _a;
                if ((_a = response.headers.get("content-type")) === null || _a === void 0 ? void 0 : _a.includes("application/json")) {
                    return response.json();
                }
                else if (response.redirected) {
                    window.open(response.url, '_blank').focus();
                }
                else {
                    return response.text().then((htmlData) => {
                        this.setAndShowModal(htmlData);
                    });
                }
            }));
        });
    }
    setAndShowModal(content) {
        const modalBody = this.modalElement.querySelector(`#${this.modalId} .modal-body`);
        this.setInnerHTML(modalBody, content);
        this.showModal();
    }
    setInnerHTML(element, html) {
        element.innerHTML = html;
        Array.from(element.querySelectorAll("script")).forEach((oldScriptElement) => {
            var _a;
            const newScriptElement = document.createElement("script");
            Array.from(oldScriptElement.attributes).forEach((attr) => {
                newScriptElement.setAttribute(attr.name, attr.value);
            });
            const scriptText = document.createTextNode(oldScriptElement.innerHTML);
            newScriptElement.appendChild(scriptText);
            (_a = oldScriptElement.parentNode) === null || _a === void 0 ? void 0 : _a.replaceChild(newScriptElement, oldScriptElement);
        });
    }
    hide() {
        this.hideModal();
    }
}
class _LegacyModal extends ModalBase {
    constructor() {
        super();
        let onModalHidden = this.onModalHidden;
        $("#" + this.modalId).on('hidden.bs.modal', function () {
            onModalHidden();
        });
    }
    createModalHtml(content, isIframe) {
        const size = isIframe
            ? this.modalSize === ModalSize.Small
                ? "auto"
                : this.modalSize === ModalSize.ExtraLarge
                    ? "65%"
                    : "auto"
            : "auto";
        const html = `
            <div id="${this.modalId}" tabindex="-1" class="modal fade" role="dialog">
                <div class="modal-dialog" style="position: auto; height: ${this.modalSize === ModalSize.ExtraLarge
            ? "95"
            : this.modalSize === ModalSize.Large
                ? "75"
                : this.modalSize === ModalSize.Fullscreen
                    ? "100"
                    : "90"}vh; width: ${size};">
                    <div class="modal-content" style="height:100%;width:auto;">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal">&times;</button>
                            <h4 class="modal-title" id="${this.modalId}-title"></h4>
                        </div>
                        <div class="modal-body" style="height:90%;width:auto;">
                            ${isIframe ? `<iframe style="border: 0px;" src="${content}" width="100%" height="97%">Waiting...</iframe>` : content}
                        </div>
                    </div>
                </div>
            </div>
        `;
        return html;
    }
    showModal() {
        $(`#${this.modalId}`).modal();
        $("iframe").on("load", () => {
            SpinnerOverlay.hide();
        });
    }
    setTitle(title) {
        $(`#${this.modalId}-title`).html(title);
    }
    showIframe(url, title, size = null) {
        this.modalSize = size || this.modalSize;
        const modalHtml = this.createModalHtml(url, true);
        $(modalHtml).appendTo($("body"));
        this.setTitle(title);
        this.showModal();
    }
    showUrl(options, title, size = null) {
        return __awaiter(this, void 0, void 0, function* () {
            this.modalSize = size || this.modalSize;
            let fetchUrl;
            let fetchOptions;
            if (options instanceof ModalUrlOptions) {
                fetchUrl = options.url;
                fetchOptions = options.requestOptions;
            }
            else {
                fetchUrl = options;
            }
            try {
                const response = yield fetch(fetchUrl, fetchOptions);
                if (response.ok) {
                    const content = yield response.text();
                    const modalHtml = this.createModalHtml(content, false);
                    $(modalHtml).appendTo($("body"));
                    this.setTitle(title);
                    this.showModal();
                }
                else {
                    console.error(`Failed to fetch content from URL: ${fetchUrl}`);
                }
            }
            catch (error) {
                console.error("An error occurred while fetching content:", error);
            }
        });
    }
    hide() {
        $(`#${this.modalId}`).modal("hide");
    }
}
class Modal {
    constructor() {
        if (bootstrapVersion === 5) {
            this.instance = new _Modal();
        }
        else {
            this.instance = new _LegacyModal();
        }
        this.instance.modalId = "jjmasterdata-modal";
        this.instance.modalSize = ModalSize.ExtraLarge;
    }
    showIframe(url, title, size = null) {
        this.instance.showIframe(url, title, size);
    }
    showUrl(options, title, size = null) {
        return __awaiter(this, void 0, void 0, function* () {
            return yield this.instance.showUrl(options, title, size);
        });
    }
    remove() {
        this.instance.hide();
        document.getElementById(this.instance.modalId).remove();
    }
    hide() {
        this.instance.hide();
    }
    get modalId() {
        return this.instance.modalId;
    }
    set modalId(value) {
        this.instance.modalId = value;
    }
    get modalTitle() {
        return this.instance.modalTitle;
    }
    set modalTitle(value) {
        this.instance.modalTitle = value;
    }
    get modalSize() {
        return this.instance.modalSize;
    }
    set modalSize(value) {
        this.instance.modalSize = value;
    }
    get modalElement() {
        return this.instance.modalElement;
    }
    set modalElement(value) {
        this.instance.modalElement = value;
    }
    get centered() {
        return this.instance.centered;
    }
    set centered(value) {
        this.instance.centered = value;
    }
    get onModalHidden() {
        return this.instance.onModalHidden;
    }
    set onModalHidden(value) {
        this.instance.onModalHidden = value;
    }
}
class DefaultModal {
    static getInstance() {
        if (this.instance === undefined) {
            this.instance = new Modal();
        }
        return this.instance;
    }
}
var defaultModal = DefaultModal.getInstance();
class popup {
    static show(title, url, size = null) {
        defaultModal.showIframe(url, title, size);
    }
    static hide() {
        defaultModal.hide();
    }
}
class PostFormValuesOptions {
}
function postFormValues(options) {
    SpinnerOverlay.show();
    const formData = new FormData(document.querySelector("form"));
    const requestOptions = {
        method: "POST",
        body: formData
    };
    fetch(options.url, requestOptions)
        .then(response => {
        var _a;
        if ((_a = response.headers.get("content-type")) === null || _a === void 0 ? void 0 : _a.includes("application/json")) {
            return response.json();
        }
        else {
            return response.text();
        }
    })
        .then(data => {
        options.success(data);
    })
        .catch(error => {
        if (options.error) {
            options.error(error);
        }
        else {
            console.error(error);
        }
    })
        .then(() => {
        SpinnerOverlay.hide();
    });
}
class SearchBoxListener {
    static listenTypeahed(selectorPrefix = String()) {
        $(selectorPrefix + "input.jj-search-box").each(function () {
            const hiddenInputId = $(this).attr("hidden-input-id");
            let queryString = $(this).attr("query-string");
            let triggerLength = $(this).attr("trigger-length");
            let numberOfItems = $(this).attr("number-of-items");
            let scrollbar = Boolean($(this).attr("scrollbar"));
            let showImageLegend = Boolean($(this).attr("show-image-legend"));
            if (triggerLength == null)
                triggerLength = "1";
            if (numberOfItems == null)
                numberOfItems = "10";
            if (scrollbar == null)
                scrollbar = false;
            if (showImageLegend == null)
                showImageLegend = false;
            const form = $("form");
            let url = new UrlBuilder().build();
            if (!url.endsWith("?"))
                url += "?";
            url += queryString;
            const jjSearchBoxSelector = "#" + hiddenInputId + "_text";
            const jjSearchBoxHiddenSelector = "#" + hiddenInputId;
            $(this).blur(function () {
                if ($(this).val() == "") {
                    FeedbackIcon.setIcon(jjSearchBoxSelector, FeedbackIcon.searchClass);
                    $(jjSearchBoxHiddenSelector).val("");
                }
                else if ($(jjSearchBoxHiddenSelector).val() == "") {
                    FeedbackIcon.setIcon(jjSearchBoxSelector, FeedbackIcon.warningClass);
                }
            });
            $(this).typeahead({
                ajax: {
                    url: url,
                    method: "POST",
                    loadingClass: "loading-circle",
                    triggerLength: triggerLength,
                    preDispatch: function () {
                        $(jjSearchBoxHiddenSelector).val("");
                        FeedbackIcon.removeAllIcons(jjSearchBoxSelector);
                        return form.serializeArray();
                    },
                },
                onSelect: function (item) {
                    const hiddenSearchBox = document.querySelector(jjSearchBoxHiddenSelector);
                    if (hiddenSearchBox)
                        hiddenSearchBox.value = item.value;
                    if (item.value != "") {
                        FeedbackIcon.setIcon(jjSearchBoxSelector, FeedbackIcon.successClass);
                    }
                },
                displayField: "name",
                valueField: "id",
                triggerLength: triggerLength,
                items: numberOfItems,
                scrollBar: scrollbar,
                item: '<li class="dropdown-item"><a href="#"></a></li>',
                highlighter: function (item) {
                    const query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, "\\$&");
                    let textSel;
                    if (showImageLegend) {
                        const parts = item.split("|");
                        textSel = parts[0].replace(new RegExp("(" + query + ")", "ig"), function ($1, match) {
                            return "<strong>" + match + "</strong>";
                        });
                        textSel = "<i class='fa fa-lg fa-fw " + parts[1] + "' style='color:" + parts[2] + ";margin-right:6px;'></i>" + textSel;
                    }
                    else {
                        textSel = item.replace(new RegExp("(" + query + ")", "ig"), function ($1, match) {
                            return "<strong>" + match + "</strong>";
                        });
                    }
                    return textSel;
                }
            });
        });
    }
}
class SliderListener {
    static listenSliders(selectorPrefix = String()) {
        let sliders = document.getElementsByClassName("jjslider");
        Array.from(sliders).forEach((slider) => {
            let sliderInput = document.getElementById(slider.id + "-value");
            document.getElementById(slider.id).addEventListener('change', function () {
                this.setAttribute('value', (this).value);
            });
            slider.oninput = function () {
                let decimalPlaces = $(this).attr("jjdecimalplaces");
                if (decimalPlaces == null)
                    decimalPlaces = "0";
                let sliderValue = (this).value;
                if (localeCode === 'pt')
                    sliderInput.value = $.number(sliderValue, decimalPlaces, ",", ".");
                else
                    sliderInput.value = $.number(sliderValue, decimalPlaces);
            };
        });
    }
    static listenInputs(selectorPrefix = String()) {
        let inputs = document.getElementsByClassName(selectorPrefix + "jjslider-value");
        Array.from(inputs).forEach((input) => {
            let slider = document.getElementById(input.id.replace("-value", ""));
            input.oninput = function () {
                slider.value = $("#" + input.id).val();
            };
        });
    }
}
class SortableListener {
    static listenSorting(selectorPrefix = String()) {
        $(selectorPrefix + ".jjsortable").sortable({
            helper: function (e, tr) {
                var originals = tr.children();
                var helper = tr.clone();
                helper.children().each(function (index) {
                    $(this).width(originals.eq(index).width());
                });
                return helper;
            },
            change: function (event, ui) {
                ui.placeholder.css({
                    visibility: "visible",
                    background: "#fbfbfb"
                });
            }
        });
    }
}
class SpinnerOverlay {
    static loadHtml() {
        if (!document.querySelector("#" + this.spinnerOverlayId)) {
            if (bootstrapVersion < 5) {
                const spinnerOverlay = document.createElement("div");
                spinnerOverlay.id = this.spinnerOverlayId;
                spinnerOverlay.innerHTML = `
            <div class="ajaxImage"></div>
            <div class="ajaxMessage">Loading...</div>
            `;
                document.body.appendChild(spinnerOverlay);
                const options = {
                    lines: 17,
                    length: 28,
                    width: 14,
                    radius: 38,
                    scale: 0.40,
                    corners: 1,
                    color: "#000",
                    opacity: 0.3,
                    rotate: 0,
                    direction: 1,
                    speed: 1.2,
                    trail: 62,
                    fps: 20,
                    zIndex: 2e9,
                    className: "spinner",
                    top: "50%",
                    left: "50%",
                    shadow: false,
                    hwaccel: false,
                    position: "absolute",
                };
                const spinner = new Spinner(options).spin();
                if (spinner.el) {
                    const spinnerOverlayElement = document.querySelector("#spinner-overlay .ajaxImage");
                    spinnerOverlayElement.parentNode.insertBefore(spinner.el, spinnerOverlayElement.nextSibling);
                }
            }
            else {
                const spinnerOverlayDiv = document.createElement('div');
                spinnerOverlayDiv.id = this.spinnerOverlayId;
                spinnerOverlayDiv.classList.add('spinner-overlay', 'text-center');
                const spinnerDiv = document.createElement('div');
                spinnerDiv.classList.add('spinner-border', 'spinner-border-lg');
                spinnerDiv.setAttribute('role', 'status');
                const spanElement = document.createElement('span');
                spanElement.classList.add('visually-hidden');
                spanElement.textContent = 'Loading...';
                spinnerDiv.appendChild(spanElement);
                spinnerOverlayDiv.appendChild(spinnerDiv);
                document.body.appendChild(spinnerOverlayDiv);
            }
        }
    }
    static show() {
        this.loadHtml();
        document.querySelector("#" + this.spinnerOverlayId).style.display = "";
    }
    static hide() {
        const overlay = document.querySelector("#" + this.spinnerOverlayId);
        if (overlay) {
            overlay.style.display = "none";
        }
    }
}
SpinnerOverlay.spinnerOverlayId = "spinner-overlay";
class TabNavListener {
    static listenTabNavs(selectorPrefix = String()) {
        $(selectorPrefix + "a.jj-tab-link").on("shown.bs.tab", function (e) {
            const link = $(e.target);
            $("#" + link.attr("jj-objectid")).val(link.attr("jj-tabindex"));
        });
    }
}
class TextAreaListener {
    static listenKeydown(selectorPrefix) {
        $(selectorPrefix + "textarea").keydown(function () {
            const jjTextArea = $(this);
            let maxLength = jjTextArea.attr("maxlength");
            let maximumLimitLabel = jjTextArea.attr("maximum-limit-of-characters-label");
            let charactersRemainingLabel = jjTextArea.attr("characters-remaining-label");
            if (isNaN(maxLength))
                maxLength = jjTextArea.attr("jjmaxlength");
            if (isNaN(maxLength))
                return;
            if (isNaN(maximumLimitLabel))
                maximumLimitLabel = "Maximum limit of {0} characters!";
            if (isNaN(charactersRemainingLabel))
                charactersRemainingLabel = "({0} characters remaining)";
            if (!isNaN(maxLength)) {
                var nId = jjTextArea.attr("id");
                var iTypedChar = jjTextArea.val().toString().length;
                if (iTypedChar > maxLength) {
                    alert(maximumLimitLabel.replace("{0}", maxLength));
                }
                charactersRemainingLabel = charactersRemainingLabel.replace("{0}", (maxLength - jjTextArea.val().toString().length));
                charactersRemainingLabel += "&nbsp;";
                if ($("#span-size-" + nId).length) {
                    $("#span-size-" + nId).html(charactersRemainingLabel);
                }
                else {
                    $("<span id='span-size-" + nId + "' class='small' style='float: right'>" + charactersRemainingLabel + "</span>").insertBefore(jjTextArea);
                }
            }
        });
    }
}
class TextFileHelper {
    static showUploadView(fieldName, title, routeContext) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        urlBuilder.addQueryParameter("fieldName", fieldName);
        const url = urlBuilder.build();
        const modalId = fieldName + "-upload-modal";
        const modal = new Modal();
        modal.modalId = modalId;
        modal.showUrl({
            url: url,
            requestOptions: { method: "POST", body: new FormData(document.querySelector("form")) }
        }, title, ModalSize.ExtraLarge).then(_ => {
            listenAllEvents("#" + modalId);
        });
    }
    static refreshInputs(id, presentationText, valueText) {
        const presentationElement = window.parent.document.getElementById(`${id}-presentation`);
        const valueElement = window.parent.document.getElementById(id);
        if (presentationElement) {
            presentationElement.value = presentationText;
        }
        if (valueElement) {
            valueElement.value = valueText;
        }
    }
}
class UploadAreaListener {
    static configureFileUpload(options) {
        const selector = "div#" + options.componentName;
        let dropzone = new window.Dropzone(selector, {
            paramName: "uploadAreaFile",
            maxFilesize: options.maxFileSize,
            uploadMultiple: options.allowMultipleFiles,
            method: "POST",
            maxFiles: options.maxFiles,
            dictDefaultMessage: options.dragDropLabel,
            dictFileTooBig: options.fileSizeErrorLabel,
            dictUploadCanceled: options.abortLabel,
            dictInvalidFileType: options.extensionNotAllowedLabel,
            clickable: true,
            parallelUploads: options.parallelUploads,
            url: options.url
        });
        const onSuccess = (file = null) => {
            if (dropzone.getQueuedFiles().length === 0) {
                const areFilesUploadedInput = document.querySelector("#" + options.componentName + "-are-files-uploaded");
                if (areFilesUploadedInput) {
                    areFilesUploadedInput.value = "1";
                }
                if (options.jsCallback) {
                    eval(options.jsCallback);
                }
            }
        };
        if (options.allowMultipleFiles) {
            dropzone.on("successmultiple", onSuccess);
        }
        else {
            dropzone.on("success", onSuccess);
        }
        if (options.allowCopyPaste) {
            document.onpaste = function (event) {
                const items = Array.from(event.clipboardData.items);
                items.forEach((item) => {
                    if (item.kind === 'file') {
                        dropzone.addFile(item.getAsFile());
                    }
                });
            };
        }
    }
    static listenFileUpload(selectorPrefix = String()) {
        document.querySelectorAll(selectorPrefix + "div.upload-area-div").forEach((element) => {
            const uploadAreaOptions = new UploadAreaOptions(element);
            this.configureFileUpload(uploadAreaOptions);
        });
    }
}
class UploadAreaOptions {
    constructor(element) {
        let dropzone = element.lastChild;
        this.componentName = dropzone.getAttribute("id");
        this.allowMultipleFiles = element.getAttribute("allow-multiple-files") === "true";
        this.jsCallback = element.getAttribute("js-callback");
        this.allowCopyPaste = element.getAttribute("allow-copy-paste") === "true";
        this.maxFileSize = Number(element.getAttribute("max-file-size"));
        this.allowDragDrop = element.getAttribute("allow-drag-drop") === "true";
        this.showFileSize = element.getAttribute("show-file-size") === "true";
        this.allowedTypes = element.getAttribute("allowed-types");
        this.fileSizeErrorLabel = element.getAttribute("file-size-error-label");
        this.dragDropLabel = element.getAttribute("drag-drop-label");
        this.abortLabel = element.getAttribute("abort-label");
        this.maxFiles = Number(element.getAttribute("max-files"));
        this.parallelUploads = Number(element.getAttribute("parallel-uploads"));
        this.extensionNotAllowedLabel = element.getAttribute("extension-not-allowed-label");
        let routeContext = element.getAttribute("route-context");
        let queryStringParams = element.getAttribute("query-string-params");
        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        const params = queryStringParams.split('&');
        for (let i = 0; i < params.length; i++) {
            const param = params[i].split('=');
            const key = decodeURIComponent(param[0]);
            const value = decodeURIComponent(param[1]);
            urlBuilder.addQueryParameter(key, value);
        }
        this.url = urlBuilder.build();
    }
}
class UploadViewHelper {
    static performFileAction(componentName, filename, action, promptMessage = null) {
        const uploadActionInput = document.getElementById("upload-view-action-" + componentName);
        const filenameInput = document.getElementById("upload-view-file-name-" + componentName);
        if (uploadActionInput && filenameInput) {
            uploadActionInput.value = action;
            filenameInput.value = action === "renameFile" ? filename + ";" + prompt(promptMessage, filename) : filename;
        }
    }
    static clearFileAction(componentName, fileName) {
        const uploadActionInput = document.getElementById("upload-view-action-" + componentName);
        const filenameInput = document.getElementById("upload-view-file-name-" + componentName);
        if (uploadActionInput && filenameInput) {
            uploadActionInput.value = String();
            filenameInput.value = String();
        }
    }
    static deleteFile(componentName, fileName, confirmationMessage, jsCallback) {
        if (confirmationMessage) {
            const confirmed = confirm(confirmationMessage);
            if (!confirmed) {
                return;
            }
        }
        this.performFileAction(componentName, fileName, "deleteFile");
        eval(jsCallback);
        this.clearFileAction(componentName, fileName);
    }
    static downloadFile(componentName, fileName, jsCallback) {
        this.performFileAction(componentName, fileName, "downloadFile");
        eval(jsCallback);
        this.clearFileAction(componentName, fileName);
    }
    static renameFile(componentName, fileName, promptMessage, jsCallback) {
        this.performFileAction(componentName, fileName, "renameFile", promptMessage);
        eval(jsCallback);
        this.clearFileAction(componentName, fileName);
    }
}
class UrlBuilder {
    constructor(url = null) {
        this.url = url;
        this.queryParameters = new Map();
    }
    addQueryParameter(key, value) {
        this.queryParameters.set(key, value);
    }
    build() {
        const form = document.querySelector("form");
        if (this.url == null) {
            this.url = form.getAttribute("action");
        }
        if (!this.url.includes("?")) {
            this.url += "?";
        }
        let isFirst = true;
        for (const [key, value] of this.queryParameters.entries()) {
            if (!isFirst) {
                this.url += "&";
            }
            this.url += `${encodeURIComponent(key)}=${encodeURIComponent(value)}`;
            isFirst = false;
        }
        return this.url;
    }
}
var jjutil = (function () {
    return {
        justNumber: function (e) {
            var strCheck = "0123456789";
            var key = "";
            let whichCode;
            if (e.which != null)
                whichCode = e.which;
            else if (e.keyCode != null)
                whichCode = e.keyCode;
            if (whichCode == 8)
                return true;
            if (whichCode == 0)
                return true;
            if ((whichCode == 99 || whichCode == 97 ||
                whichCode == 118 || whichCode == 120 ||
                whichCode == 67 || whichCode == 88 ||
                whichCode == 86 || whichCode == 65) &&
                (e.ctrlKey === true || e.metaKey === true)) {
                return true;
            }
            key = String.fromCharCode(whichCode);
            if (strCheck.indexOf(key) == -1)
                return false;
        },
        justDecimal: function (e) {
            var strCheck = "-0123456789.,";
            var key = "";
            let whichCode;
            if (e.which != null)
                whichCode = e.which;
            else if (e.keyCode != null)
                whichCode = e.keyCode;
            if (whichCode == 8)
                return true;
            if (whichCode == 0)
                return true;
            if ((whichCode == 99 || whichCode == 120 || whichCode == 118 || whichCode == 97 ||
                whichCode == 67 || whichCode == 88 || whichCode == 86 || whichCode == 65) &&
                (e.ctrlKey === true || e.metaKey === true)) {
                return true;
            }
            key = String.fromCharCode(whichCode);
            if (strCheck.indexOf(key) == -1)
                return false;
        },
        gotoNextFocus: function (currentId) {
            var self = $("#" + currentId);
            var form = self.parents("form:eq(0)");
            var focusable = form.find("input,a.btn,select,button[type=submit]").filter(":visible");
            var next = focusable.eq(focusable.index(self) + 1);
            if (next.length) {
                if (next.is(":disabled")) {
                    for (let i = 2; i < 1000; i++) {
                        next = focusable.eq(focusable.index(self) + i);
                        if (!next.is(":disabled") && next.is(":visible"))
                            break;
                    }
                }
                next.focus();
                next.select();
            }
        },
        replaceEntertoTab: function (objid) {
            $("#" + objid + " input").on("keypress", function (e) {
                if (e.keyCode == 13) {
                    jjutil.gotoNextFocus($(this).attr("id"));
                    return false;
                }
            });
        },
        animateValue: function (id, start, end, duration) {
            if (start === end)
                return;
            var range = end - start;
            var current = start;
            var increment = end > start ? 1 : -1;
            var stepTime = Math.abs(Math.floor(duration / range));
            var incrementValue = increment;
            if (stepTime == 0) {
                incrementValue = increment * Math.abs(Math.ceil(range / duration));
                stepTime = 1;
            }
            var obj = document.getElementById(id);
            var timer = setInterval(function () {
                current = parseInt(obj.innerHTML);
                current += incrementValue;
                if ((current >= end && increment > 0) ||
                    (current <= end && increment < 0)) {
                    obj.innerHTML = end.toString();
                    clearInterval(timer);
                }
                else {
                    obj.innerHTML = current.toString();
                }
            }, stepTime);
        }
    };
})();
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
//# sourceMappingURL=jjmasterdata.js.map