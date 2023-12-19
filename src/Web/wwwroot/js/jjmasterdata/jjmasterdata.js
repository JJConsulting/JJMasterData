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
class ActionHelper {
    static executeSqlCommand(componentName, encryptedActionMap, encryptedRouteContext, confirmMessage) {
        if (confirmMessage) {
            const result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }
        const gridViewActionInput = document.querySelector("#grid-view-action-map-" + componentName);
        const formViewActionInput = document.querySelector("#form-view-action-map-" + componentName);
        if (gridViewActionInput) {
            gridViewActionInput.value = encryptedActionMap;
        }
        else if (formViewActionInput) {
            formViewActionInput.value = encryptedActionMap;
        }
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", encryptedRouteContext);
        postFormValues({ url: urlBuilder.build(), success: data => {
                document.getElementById(componentName).innerHTML = data;
            } });
    }
    static executeRedirectAction(componentName, routeContext, encryptedActionMap, confirmationMessage) {
        if (confirmationMessage) {
            const result = confirm(confirmationMessage);
            if (!result) {
                return false;
            }
        }
        const gridViewActionInput = document.querySelector("#grid-view-action-map-" + componentName);
        const formViewActionInput = document.querySelector("#form-view-action-map-" + componentName);
        if (formViewActionInput) {
            formViewActionInput.value = encryptedActionMap;
        }
        else {
            const newFormInput = document.createElement("input");
            newFormInput.id = "form-view-action-map-" + componentName;
            newFormInput.name = "form-view-action-map-" + componentName;
            newFormInput.type = "hidden";
            newFormInput.value = encryptedActionMap;
            document.querySelector('form').appendChild(newFormInput);
        }
        if (gridViewActionInput) {
            gridViewActionInput.value = encryptedActionMap;
        }
        else {
            const newGridInput = document.createElement("input");
            newGridInput.id = "grid-view-action-map-" + componentName;
            newGridInput.name = "grid-view-action-map-" + componentName;
            newGridInput.type = "hidden";
            newGridInput.value = encryptedActionMap;
            document.querySelector('form').appendChild(newGridInput);
        }
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
                if (data.urlAsModal) {
                    if (data.isIframe) {
                        defaultModal.showIframe(data.urlRedirect, data.modalTitle, data.modalSize);
                    }
                    else {
                        defaultModal.showUrl(data.urlRedirect, data.modalTitle, data.modalSize);
                    }
                }
                else {
                    window.location.href = data.urlRedirect;
                }
            }
        });
    }
    static executeInternalRedirect(url, modalSize, confirmationMessage) {
        if (confirmationMessage) {
            if (!confirm(confirmationMessage)) {
                return false;
            }
        }
        defaultModal.showIframe(url, "", modalSize);
    }
    static executeActionData(actionData) {
        const { componentName, actionMap, modalTitle, modalRouteContext, gridViewRouteContext, formViewRouteContext, isSubmit, confirmationMessage } = actionData;
        if (confirmationMessage) {
            if (!confirm(confirmationMessage)) {
                return false;
            }
        }
        const gridViewActionInput = document.querySelector("#grid-view-action-map-" + componentName);
        const formViewActionInput = document.querySelector("#form-view-action-map-" + componentName);
        if (gridViewActionInput) {
            gridViewActionInput.value = "";
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
            SpinnerOverlay.show();
            const requestOptions = getRequestOptions();
            modal.showUrl({
                url: urlBuilder.build(), requestOptions: requestOptions
            }, modalTitle).then(function (data) {
                SpinnerOverlay.hide();
                listenAllEvents("#" + modal.modalId + " ");
                if (typeof data === "object") {
                    if (data.closeModal) {
                        GridViewHelper.refresh(componentName, gridViewRouteContext);
                        modal.remove();
                    }
                }
            });
        }
        else {
            if (!isSubmit) {
                const urlBuilder = new UrlBuilder();
                urlBuilder.addQueryParameter("routeContext", formViewRouteContext);
                postFormValues({ url: urlBuilder.build(), success: (data) => {
                        if (typeof data === "string") {
                            HTMLHelper.setInnerHTML(componentName, data);
                            listenAllEvents("#" + componentName);
                        }
                        else {
                            if (data.jsCallback) {
                                eval(data.jsCallback);
                            }
                        }
                    } });
            }
            else {
                document.forms[0].requestSubmit();
            }
        }
    }
    static executeAction(actionDataJson) {
        const actionData = JSON.parse(actionDataJson);
        return this.executeActionData(actionData);
    }
    static hideActionModal(componentName) {
        const modal = new Modal();
        modal.modalId = componentName + "-modal";
        modal.remove();
    }
    static launchUrl(url, isModal, title, confirmationMessage, modalSize = 1) {
        if (confirmationMessage) {
            const result = confirm(confirmationMessage);
            if (!result) {
                return false;
            }
        }
        if (isModal) {
            popup.show(title, url, modalSize);
        }
        else {
            window.location.href = url;
        }
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
        const datetimeInputs = document.querySelectorAll(prefixSelector + ".jjform-datetime");
        datetimeInputs.forEach(function (div) {
            flatpickr(div, {
                enableTime: true,
                wrap: true,
                allowInput: true,
                altInput: false,
                time_24hr: true,
                mode: div.firstElementChild.getAttribute("multiple-dates") === "True" ? "multiple" : "single",
                dateFormat: localeCode === "pt" ? "d/m/Y H:i" : "m/d/Y H:i",
                onOpen: function (selectedDates, dateStr, instance) {
                    if (instance.input.getAttribute("autocompletePicker") === "True") {
                        instance.setDate(Date.now());
                    }
                },
                locale: localeCode,
            });
        });
        const dateInputs = document.querySelectorAll(prefixSelector + ".jjform-date");
        dateInputs.forEach(function (div) {
            flatpickr(div, {
                enableTime: false,
                wrap: true,
                allowInput: true,
                altInput: false,
                mode: div.firstElementChild.getAttribute("multiple-dates") === "True" ? "multiple" : "single",
                dateFormat: localeCode === "pt" ? "d/m/Y" : "m/d/Y",
                onOpen: function (selectedDates, dateStr, instance) {
                    if (instance.input.getAttribute("autocompletePicker") === "True") {
                        instance.setDate(Date.now());
                    }
                },
                locale: localeCode,
            });
        });
        const hourInputs = document.querySelectorAll(prefixSelector + ".jjform-hour");
        hourInputs.forEach(function (div) {
            flatpickr(div, {
                enableTime: true,
                wrap: true,
                noCalendar: true,
                allowInput: true,
                altInput: false,
                dateFormat: "H:i",
                mode: div.firstElementChild.getAttribute("multiple-dates") === "True" ? "multiple" : "single",
                time_24hr: true,
                onOpen: function (selectedDates, dateStr, instance) {
                    if (instance.input.getAttribute("autocompletePicker") === "True") {
                        instance.setDate(Date.now());
                    }
                },
                locale: localeCode,
            });
        });
    }
}
class CheckboxHelper {
    static check(name) {
        const checkbox = document.querySelector(`#${name}-checkbox`);
        if (checkbox === null || checkbox === void 0 ? void 0 : checkbox.checked) {
            document.querySelector(`#${name}`).value = "true";
        }
        else {
            document.querySelector(`#${name}`).value = "false";
        }
    }
}
class CodeMirrorWrapperOptions {
}
class CodeMirrorWrapper {
    static isCodeMirrorConfigured(elementId) {
        const textArea = document.querySelector("#" + elementId);
        return textArea.codeMirrorInstance != null;
    }
    static setupCodeMirror(elementId, options) {
        const textArea = document.querySelector("#" + elementId + "-ExpressionValue");
        if (!textArea)
            return;
        if (this.isCodeMirrorConfigured(elementId + "-ExpressionValue"))
            return;
        const codeMirrorTextArea = CodeMirror.fromTextArea(textArea, {
            mode: options.mode,
            indentWithTabs: true,
            smartIndent: true,
            lineNumbers: !options.singleLine,
            autofocus: false,
            autohint: true,
            extraKeys: { "Ctrl-Space": "autocomplete" }
        });
        if (options.singleLine) {
            codeMirrorTextArea.setSize(null, 29);
            codeMirrorTextArea.on("beforeChange", function (instance, change) {
                const newText = change.text.join("").replace(/\n/g, "");
                change.update(change.from, change.to, [newText]);
                return true;
            });
        }
        else {
            codeMirrorTextArea.setSize(null, 250);
        }
        CodeMirror.registerHelper('hint', 'hintList', function (_) {
            const cur = codeMirrorTextArea.getCursor();
            return {
                list: options.hintList,
                from: CodeMirror.Pos(cur.line, cur.ch),
                to: CodeMirror.Pos(cur.line, cur.ch)
            };
        });
        codeMirrorTextArea.on("keyup", function (cm, event) {
            if (!cm.state.completionActive && event.key === options.hintKey) {
                CodeMirror.commands.autocomplete(cm, CodeMirror.hint.hintList, { completeSingle: false });
            }
        });
        textArea.codeMirrorInstance = codeMirrorTextArea;
        setTimeout(() => {
            codeMirrorTextArea.refresh();
        }, 250);
    }
}
class CollapsePanelListener {
    static listen(componentName) {
        let nameSelector = "#" + componentName;
        let collapseSelector = '#' + componentName + '-is-open';
        let collapseElement = document.querySelector(nameSelector);
        if (bootstrapVersion === 5) {
            collapseElement.addEventListener("hidden.bs.collapse", function () {
                document.querySelector(collapseSelector).value = "0";
            });
            collapseElement.addEventListener("show.bs.collapse", function () {
                document.querySelector(collapseSelector).value = "1";
            });
        }
        else {
            $(nameSelector).on('hidden.bs.collapse', function () {
                $(collapseSelector).val("0");
            });
            $(nameSelector).on('show.bs.collapse', function () {
                $(collapseSelector).val("1");
            });
        }
    }
}
class DataDictionaryUtils {
    static deleteAction(actionName, url, confirmationMessage) {
        let confirmed = confirm(confirmationMessage);
        if (confirmed == true) {
            fetch(url, {
                method: "POST",
            })
                .then(response => response.json())
                .then(data => {
                if (data.success) {
                    document.getElementById(actionName).remove();
                }
            });
        }
    }
    static sortAction(context, url, errorMessage) {
        $("#sortable-" + context).sortable({
            update: function () {
                const order = $(this).sortable('toArray');
                const formData = new FormData();
                formData.append('fieldsOrder', order);
                formData.append('context', context);
                fetch(url, {
                    method: 'POST',
                    body: formData,
                })
                    .then(function (response) {
                    return response.json();
                })
                    .then(function (data) {
                    if (!data.success) {
                        messageBox.show('JJMasterData', errorMessage, 4);
                    }
                });
            }
        }).disableSelection();
    }
    static toggleActionEnabled(visibility, url, errorMessage) {
        const formData = new FormData();
        formData.append('visibility', visibility.toString());
        fetch(url, {
            method: "POST",
            body: formData,
        })
            .then(response => response.json())
            .then(data => {
            if (!data.success) {
                messageBox.show("JJMasterData", errorMessage, 4);
            }
        });
    }
    static postAction(url) {
        window.parent.document.forms[0].requestSubmit();
    }
    static exportElement(id, url, validationMessage) {
        const values = document.querySelector('#grid-view-selected-rows-' + id).value;
        if (values === "") {
            messageBox.show("JJMasterData", validationMessage, 3);
            return false;
        }
        SpinnerOverlay.show();
        const requestOptions = getRequestOptions();
        fetch(url, requestOptions).then((response) => __awaiter(this, void 0, void 0, function* () {
            const blob = yield response.blob();
            const contentDisposition = response.headers.get('Content-Disposition');
            const fileNameMatch = /filename="(.*)"/.exec(contentDisposition);
            const fileName = fileNameMatch[1];
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            a.click();
            window.URL.revokeObjectURL(url);
            SpinnerOverlay.hide();
        }));
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
        const requestOptions = getRequestOptions();
        fetch(urlBuilder.build(), requestOptions)
            .then(response => response.text()).then((html) => __awaiter(this, void 0, void 0, function* () {
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
        const requestOptions = getRequestOptions();
        DataImportationModal.getInstance().showUrl({
            url: urlBuilder.build(),
            requestOptions: requestOptions
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
                const requestOptions = getRequestOptions();
                DataImportationModal.getInstance().showUrl({
                    url: urlBuilder.build(),
                    requestOptions: requestOptions
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
        urlBuilder.addQueryParameter("fieldName", fieldName);
        urlBuilder.addQueryParameter("routeContext", routeContext);
        const form = document.querySelector("form");
        postFormValues({
            url: urlBuilder.build(),
            success: data => {
                if (typeof data === "string") {
                    HTMLHelper.setOuterHTML(componentName, data);
                    listenAllEvents("#" + componentName);
                    jjutil.gotoNextFocus(fieldName);
                }
                else {
                    if (data.jsCallback) {
                        eval(data.jsCallback);
                    }
                }
            }
        });
    }
}
function applyDecimalPlaces(element) {
    var _a, _b, _c;
    if (element.getAttribute("type") == "number")
        return;
    let decimalPlaces = (_a = element.getAttribute("jj-decimal-places")) !== null && _a !== void 0 ? _a : 2;
    let decimalSeparator = (_b = element.getAttribute("jj-decimal-separator")) !== null && _b !== void 0 ? _b : '.';
    let groupSeparator = (_c = element.getAttribute("jj-group-separator")) !== null && _c !== void 0 ? _c : ',';
    new AutoNumeric(element, {
        decimalCharacter: decimalSeparator,
        digitGroupSeparator: groupSeparator,
        decimalPlaces: decimalPlaces
    });
}
function listenExpressionType(name, hintList, isBoolean) {
    document.getElementById(name + '-ExpressionType').addEventListener('change', function () {
        const selectedType = this.value;
        const expressionValueInput = document.getElementById(name + '-ExpressionValue');
        const expressionValueEditor = document.getElementById(name + '-ExpressionValueEditor');
        if (selectedType === 'val' && isBoolean === true) {
            const div = document.createElement('div');
            div.classList.add('form-switch', 'form-switch-md', 'form-check');
            const expressionValueInputName = name + '-ExpressionValue';
            const input = document.createElement('input');
            input.name = expressionValueInputName;
            input.id = expressionValueInputName;
            input.setAttribute("value", "false");
            input.setAttribute("hidden", "hidden");
            const checkbox = document.createElement('input');
            checkbox.name = name + '-ExpressionValue-checkbox';
            checkbox.id = name + '-ExpressionValue-checkbox';
            checkbox.type = 'checkbox';
            checkbox.setAttribute("role", 'switch');
            checkbox.setAttribute("value", "false");
            checkbox.setAttribute("onchange", `CheckboxHelper.check('${expressionValueInputName}')`);
            checkbox.classList.add('form-check-input');
            div.appendChild(input);
            div.appendChild(checkbox);
            expressionValueEditor.innerHTML = div.outerHTML;
        }
        else {
            const textArea = document.createElement('textarea');
            textArea.setAttribute('name', name + '-ExpressionValue');
            textArea.setAttribute('id', name + '-ExpressionValue');
            textArea.setAttribute('class', 'form-control');
            textArea.innerText = expressionValueInput.value;
            expressionValueEditor.innerHTML = textArea.outerHTML;
            CodeMirrorWrapper.setupCodeMirror(name, { mode: 'text/x-sql', singleLine: true, hintList: hintList, hintKey: '{' });
        }
    });
}
class FeedbackIcon {
    static removeAllIcons(selector) {
        const elements = window.parent.document.querySelectorAll(selector);
        elements === null || elements === void 0 ? void 0 : elements.forEach(element => {
            element.classList.remove(FeedbackIcon.successClass, FeedbackIcon.warningClass, FeedbackIcon.searchClass, FeedbackIcon.errorClass);
        });
    }
    static setIcon(selector, iconClass) {
        this.removeAllIcons(selector);
        const elements = window.parent.document.querySelectorAll(selector);
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
    static showInsertSuccess(componentName, gridViewRouteContext) {
        const insertAlertDiv = document.getElementById(`insert-alert-div-${componentName}`);
        setTimeout(function () {
            insertAlertDiv.style.opacity = "0";
        }, 1000);
        setTimeout(function () {
            insertAlertDiv.style.display = "none";
        }, 3000);
        GridViewHelper.refresh(componentName, gridViewRouteContext);
    }
    static refreshFormView(componentName, routeContext) {
        const url = new UrlBuilder().addQueryParameter("routeContext", routeContext).build();
        postFormValues({
            url: url,
            success: (data) => {
                HTMLHelper.setInnerHTML(componentName, data);
                listenAllEvents("#" + componentName);
            }
        });
    }
    static setPageState(componentName, pageState, routeContext) {
        document.querySelector(`#form-view-page-state-${componentName}`).value = pageState.toString();
        document.querySelector(`#form-view-action-map-${componentName}`).value = String();
        this.refreshFormView(componentName, routeContext);
    }
    static setPanelState(componentName, pageState, routeContext) {
        document.querySelector(`#form-view-panel-state-${componentName}`).value = pageState.toString();
        document.querySelector(`#form-view-action-map-${componentName}`).value = String();
        this.refreshFormView(componentName, routeContext);
    }
    static insertSelection(componentName, insertValues, routeContext) {
        document.querySelector(`#form-view-insert-selection-values-${componentName}`).value = insertValues;
        this.refreshFormView(componentName, routeContext);
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
    static filter(gridViewName, routeContext) {
        document.querySelector("#grid-view-filter-action-" + gridViewName).value = "filter";
        document.querySelector("#grid-view-action-map-" + gridViewName).value = "";
        document.querySelector("#grid-view-page-" + gridViewName).value = "1";
        GridViewHelper.clearCurrentFormAction(gridViewName);
        GridViewHelper.refreshGrid(gridViewName, routeContext);
    }
    static reload(gridViewName, filterPanelName, routeContext) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        document.querySelector("#grid-view-filter-action-" + gridViewName).value = "filter";
        postFormValues({
            url: urlBuilder.build(),
            success: (content) => {
                document.getElementById(filterPanelName).innerHTML = content;
            }
        });
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
        document.querySelector("#grid-view-filter-action-" + componentName).value = "clear";
        document.querySelector("#grid-view-action-map-" + componentName).value = "";
        GridViewHelper.clearCurrentFormAction(componentName);
    }
    static clearFilter(componentName, routeContext) {
        this.clearFilterInputs(componentName);
        GridViewHelper.refreshGrid(componentName, routeContext);
    }
    static searchOnDOM(componentName, oDom) {
        const value = $(oDom).val().toString().toLowerCase();
        $("#" + componentName + "-table" + " tr").filter(function () {
            const textValues = $(this).clone().find('.bootstrap-select, .selectpicker, select').remove().end().text();
            let isSearch = textValues.toLowerCase().indexOf(value) > -1;
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
            $("#infotext_" + componentName).css("display", "none");
            $("ul.pagination").css("display", "none");
        }
        else {
            $("#infotext_" + componentName).css("display", "");
            $("ul.pagination").css("display", "");
        }
    }
}
class GridViewHelper {
    static openSettingsModal(componentName, encryptedActionMap) {
        const gridViewActionInput = document.getElementById("grid-view-action-map-" + componentName);
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
        const modalId = "config-modal-" + componentName;
        const modalElement = document.getElementById("config-modal-" + componentName);
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
        if (modalElement) {
            const modal = new Modal();
            modal.modalId = modalId;
            modal.hide();
        }
    }
    static sortGridValues(componentName, routeContext, field) {
        const tableOrderElement = document.querySelector("#grid-view-order-" + componentName);
        if (field + " ASC" === tableOrderElement.value)
            tableOrderElement.value = field + " DESC";
        else
            tableOrderElement.value = field + " ASC";
        document.querySelector("#grid-view-action-map-" + componentName).value = "";
        this.clearCurrentFormAction(componentName);
        GridViewHelper.refreshGrid(componentName, routeContext);
    }
    static sortMultItems(componentName, routeContext) {
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
        let modal = new Modal();
        modal.modalId = componentName + "-sort-modal";
        modal.hide();
        this.clearCurrentFormAction(componentName);
        GridViewHelper.refreshGrid(componentName, routeContext);
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
        const currentGridAction = document.querySelector("#grid-view-action-map-" + componentName);
        if (currentGridAction)
            currentGridAction.value = "";
    }
    static paginate(componentName, routeContext, currentPage) {
        this.setCurrentGridPage(componentName, currentPage);
        this.clearCurrentGridAction(componentName);
        this.clearCurrentFormAction(componentName);
        GridViewHelper.refreshGrid(componentName, routeContext);
    }
    static jumpToPage(componentName, routeContext) {
        const jumpToPageInput = document.querySelector("#" + componentName + "-jump-to-page-input");
        const numericPage = Number(jumpToPageInput.value);
        if (isNaN(numericPage) || numericPage <= 0 || numericPage > Number(jumpToPageInput.max)) {
            jumpToPageInput.classList.add("is-invalid");
            return;
        }
        this.paginate(componentName, routeContext, numericPage);
    }
    static showJumpToPage(jumpToPageName) {
        const jumpToPageInput = $("#" + jumpToPageName);
        jumpToPageInput.val(null);
        jumpToPageInput.animate({ width: 'toggle' }, null, function () {
            jumpToPageInput.removeClass("is-invalid");
        });
    }
    static refresh(componentName, routeContext) {
        this.setCurrentGridPage(componentName, String());
        this.clearCurrentGridAction(componentName);
        this.clearCurrentFormAction(componentName);
        GridViewHelper.refreshGrid(componentName, routeContext);
    }
    static submitGrid(componentName) {
        this.setCurrentGridPage(componentName, String());
        this.clearCurrentGridAction(componentName);
        this.clearCurrentFormAction(componentName);
        document.forms[0].submit();
    }
    static refreshGrid(componentName, routeContext) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        postFormValues({
            url: urlBuilder.build(),
            success: function (data) {
                const gridViewTableElement = document.querySelector("#grid-view-table-" + componentName);
                const filterActionElement = document.querySelector("#grid-view-filter-action-" + componentName);
                if (gridViewTableElement) {
                    gridViewTableElement.outerHTML = data;
                    listenAllEvents("#" + componentName);
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
    static selectItem(componentName, inputElement) {
        const valuesInput = document.getElementById("grid-view-selected-rows-" + componentName);
        const values = valuesInput.value.toString();
        let valuesList = [];
        if (inputElement.id === `${componentName}-checkbox-select-all-rows`) {
            return;
        }
        if (values.length > 0) {
            valuesList = values.split(",");
        }
        if (inputElement.checked) {
            if (valuesList.indexOf(inputElement.value) < 0) {
                valuesList.push(inputElement.value);
            }
        }
        else {
            valuesList = valuesList.filter((item) => item !== inputElement.value);
        }
        valuesInput.value = valuesList.join(",");
        let textInfo;
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
    static selectAllAtSamePage(componentName) {
        const checkboxes = document.querySelectorAll(`#${componentName} td.jj-checkbox input`);
        const selectAllCheckbox = document.querySelector(`#${componentName}-checkbox-select-all-rows`);
        const isSelectAllChecked = selectAllCheckbox.checked;
        checkboxes.forEach(function (checkbox) {
            if (!checkbox.disabled) {
                checkbox.checked = isSelectAllChecked;
                const event = new Event('change');
                checkbox.dispatchEvent(event);
            }
        });
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
        const checkboxes = document.querySelectorAll(`#grid-view-table-${componentName} .jj-checkbox input:not(:disabled)`);
        checkboxes.forEach(checkbox => checkbox.checked = true);
        const selectedRowsInput = document.getElementById("grid-view-selected-rows-" + componentName);
        selectedRowsInput.value = values.join(",");
        const selectedText = document.getElementById("selected-text-" + componentName);
        selectedText.textContent = selectedText.getAttribute("multiple-records-selected-label").replace("{0}", values.length.toString());
    }
    static unSelectAll(componentName) {
        const checkboxes = document.querySelectorAll(`#grid-view-table-${componentName} .jj-checkbox input:not(:disabled)`);
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
class HTMLHelper {
    static setOuterHTML(elementName, html) {
        const targetElement = document.getElementById(elementName);
        if (!targetElement) {
            throw new Error(`Element not found: ${elementName}`);
        }
        targetElement.outerHTML = html;
        this.makeScriptsExecutable(document.getElementById(elementName));
    }
    static setInnerHTML(element, html) {
        const targetElement = typeof element === "string" ? document.getElementById(element) : element;
        if (!targetElement) {
            throw new Error(`Element not found: ${element}`);
        }
        targetElement.innerHTML = html;
        this.makeScriptsExecutable(targetElement);
    }
    static makeScriptsExecutable(element) {
        element.querySelectorAll("script").forEach(script => {
            var _a;
            const clone = document.createElement("script");
            for (const attr of script.attributes) {
                clone.setAttribute(attr.name, attr.value);
            }
            clone.text = script.innerHTML;
            (_a = script.parentNode) === null || _a === void 0 ? void 0 : _a.replaceChild(clone, script);
        });
    }
}
document.addEventListener("DOMContentLoaded", function () {
    listenAllEvents();
});
const listenAllEvents = (selectorPrefix = String()) => {
    selectorPrefix += " ";
    $(selectorPrefix + ".selectpicker").selectpicker({
        iconBase: bootstrapVersion === 5 ? 'fa' : 'glyphicon'
    });
    if (bootstrapVersion === 3) {
        $(selectorPrefix + "input[type=checkbox][data-toggle^=toggle]").bootstrapToggle();
    }
    CalendarListener.listen(selectorPrefix);
    TextAreaListener.listenKeydown(selectorPrefix);
    SearchBoxListener.listenTypeahed(selectorPrefix);
    LookupListener.listenChanges(selectorPrefix);
    SortableListener.listenSorting(selectorPrefix);
    UploadAreaListener.listenFileUpload(selectorPrefix);
    TabNavListener.listenTabNavs(selectorPrefix);
    SliderListener.listenSliders(selectorPrefix);
    SliderListener.listenInputs(selectorPrefix);
    Inputmask().mask(document.querySelectorAll("input"));
    if (bootstrapVersion === 5) {
        TooltipListener.listen(selectorPrefix);
    }
    document.querySelectorAll(selectorPrefix + ".jj-numeric").forEach(applyDecimalPlaces);
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
        const idInput = window.parent.document.querySelector("#" + fieldName);
        idInput.value = id;
        const descriptionInput = window.parent.document.querySelector("#" + fieldName + "-description");
        if (descriptionInput) {
            descriptionInput.value = description;
        }
        FeedbackIcon.setIcon(fieldName, FeedbackIcon.successClass);
        window.parent.defaultModal.remove();
    }
}
class LookupListener {
    static listenChanges(selectorPrefix = String()) {
        const lookupInputs = document.querySelectorAll(selectorPrefix + "input.jj-lookup");
        lookupInputs.forEach(lookupInput => {
            let lookupId = lookupInput.id;
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("routeContext", lookupInput.getAttribute("route-context"));
            urlBuilder.addQueryParameter("fieldName", lookupInput.getAttribute("lookup-field-name"));
            const lookupDescriptionUrl = urlBuilder.build();
            const lookupIdSelector = "#" + lookupId;
            const lookupDescriptionSelector = lookupIdSelector + "-description";
            const lookupIdInput = document.querySelector(lookupIdSelector);
            const lookupDescriptionInput = document.querySelector(lookupDescriptionSelector);
            lookupInput.addEventListener("blur", function () {
                FeedbackIcon.removeAllIcons(lookupDescriptionSelector);
                postFormValues({
                    url: lookupDescriptionUrl,
                    success: (data) => {
                        if (!data.description) {
                            FeedbackIcon.setIcon(lookupIdSelector, FeedbackIcon.warningClass);
                            lookupDescriptionInput.value = String();
                        }
                        else {
                            FeedbackIcon.setIcon(lookupIdSelector, FeedbackIcon.successClass);
                            lookupIdInput.value = data.id;
                            if (lookupDescriptionInput) {
                                lookupDescriptionInput.value = data.description;
                            }
                        }
                    },
                    error: (_) => {
                        FeedbackIcon.setIcon(lookupIdSelector, FeedbackIcon.errorClass);
                        lookupDescriptionInput.value = String();
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
        if (bootstrapVersion < 5) {
            $(MessageBox.jQueryModalId)
                .modal()
                .on("shown.bs.modal", function () {
                $(MessageBox.jQueryModalButton1Id).focus();
            });
        }
        else {
            const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById(MessageBox.modalId), {});
            modal.show();
            document.addEventListener("shown.bs.modal", function () {
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
        MessageBox.hide();
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
        if (bootstrapVersion >= 4) {
            html += "        <h4 id=\"site-modal-title\" class=\"modal-title\"></h4>\r\n";
        }
        else if (bootstrapVersion >= 5) {
            html +=
                '        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>\r\n';
        }
        else if (bootstrapVersion == 3) {
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
        if (bootstrapVersion == 3) {
            html += '        <button type="button" id="site-modal-btn1" class="btn btn-default" data-dismiss="modal"></button>\r\n';
            html += '        <button type="button" id="site-modal-btn2" class="btn btn-default" data-dismiss="modal"></button>\r\n';
        }
        else if (bootstrapVersion == 4) {
            html += '        <button type="button" id="site-modal-btn1" class="btn btn-secondary" data-dismiss="modal"></button>\r\n';
            html += '        <button type="button" id="site-modal-btn2" class="btn btn-secondary" data-dismiss="modal"></button>\r\n';
        }
        else {
            html += '        <button type="button" id="site-modal-btn1" class="btn btn-secondary" data-bs-dismiss="modal"></button>\r\n';
            html += '        <button type="button" id="site-modal-btn2" class="btn btn-secondary" data-bs-dismiss="modal"></button>\r\n';
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
const messageBox = MessageBox;
var ModalSize;
(function (ModalSize) {
    ModalSize[ModalSize["Fullscreen"] = 0] = "Fullscreen";
    ModalSize[ModalSize["ExtraLarge"] = 1] = "ExtraLarge";
    ModalSize[ModalSize["Large"] = 2] = "Large";
    ModalSize[ModalSize["Default"] = 3] = "Default";
    ModalSize[ModalSize["Small"] = 4] = "Small";
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
        super();
        this.modalSizeCssClass = {
            Default: "jj-modal-default",
            ExtraLarge: "jj-modal-xl",
            Large: "jj-modal-lg",
            Small: "jj-modal-sm",
            Fullscreen: "modal-fullscreen",
        };
    }
    get modalTitle() {
        return this._modalTitle;
    }
    set modalTitle(value) {
        this._modalTitle = value;
        document.getElementById(`${this.modalId}-title`).innerText = value;
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
            <h5 class="modal-title" id="${this.modalId}-title">${this.modalTitle}</h5>
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
                document.getElementById(this.modalId).remove();
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
        this._modalTitle = title;
        this.modalSize = size !== null && size !== void 0 ? size : ModalSize.Default;
        this.createModalElement();
        const modalBody = this.modalElement.querySelector(".modal-body");
        modalBody.innerHTML = `<iframe src="${url}" class="modal-iframe"></iframe>`;
        this.showModal();
    }
    showUrl(modalOptions, title, size = null) {
        return __awaiter(this, void 0, void 0, function* () {
            this._modalTitle = title;
            this.modalSize = size !== null && size !== void 0 ? size : ModalSize.Default;
            this.createModalElement();
            let fetchUrl;
            let fetchOptions;
            if (typeof modalOptions === 'object' && 'url' in modalOptions) {
                fetchUrl = modalOptions.url;
                fetchOptions = modalOptions.requestOptions;
            }
            else {
                fetchUrl = modalOptions;
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
        HTMLHelper.setInnerHTML(modalBody, content);
        this.showModal();
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
    get modalTitle() {
        return this._modalTitle;
    }
    set modalTitle(value) {
        this._modalTitle = value;
        document.getElementById(`${this.modalId}-title`).innerText = value;
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
        const modalIdSelector = `#${this.modalId}`;
        if ($(modalIdSelector).length) {
            $(modalIdSelector).remove();
        }
        const $form = $("form");
        if ($form.length) {
            $(modalHtml).appendTo($form);
        }
        else {
            $(modalHtml).appendTo($("body"));
        }
        this.setTitle(title);
        this.showModal();
    }
    showUrl(options, title, size = null) {
        return __awaiter(this, void 0, void 0, function* () {
            this.modalSize = size || this.modalSize;
            let fetchUrl;
            let fetchOptions;
            if (typeof options === 'object' && 'url' in options) {
                fetchUrl = options.url;
                fetchOptions = options.requestOptions;
            }
            else {
                fetchUrl = options;
            }
            try {
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
                            const modalHtml = this.createModalHtml(htmlData, false);
                            const modalIdSelector = `#${this.modalId}`;
                            if ($(modalIdSelector).length) {
                                $(modalIdSelector).remove();
                            }
                            const $form = $("form");
                            if ($form.length) {
                                $(modalHtml).appendTo($form);
                            }
                            else {
                                $(modalHtml).appendTo($("body"));
                            }
                            this.setTitle(title);
                            this.showModal();
                        });
                    }
                }));
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
var PageState;
(function (PageState) {
    PageState[PageState["List"] = 1] = "List";
    PageState[PageState["View"] = 2] = "View";
    PageState[PageState["Insert"] = 3] = "Insert";
    PageState[PageState["Update"] = 4] = "Update";
    PageState[PageState["Filter"] = 5] = "Filter";
    PageState[PageState["Import"] = 6] = "Import";
    PageState[PageState["Delete"] = 7] = "Delete";
    PageState[PageState["AuditLog"] = 8] = "AuditLog";
})(PageState || (PageState = {}));
class PostFormValuesOptions {
}
function getRequestOptions() {
    const formData = new FormData(document.querySelector("form"));
    return {
        method: "POST",
        body: formData
    };
}
function postFormValues(options) {
    SpinnerOverlay.show();
    const requestOptions = getRequestOptions();
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
            let urlBuilder = new UrlBuilder();
            for (const pair of queryString.split("&")) {
                const [key, value] = pair.split("=");
                if (key && value) {
                    urlBuilder.addQueryParameter(key, value);
                }
            }
            const url = urlBuilder.build();
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
                item: '<li><a class="dropdown-item" href="#"></a></li>',
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
        let sliders = document.querySelectorAll(selectorPrefix + ".jjslider");
        Array.from(sliders).forEach((slider) => {
            const sliderInput = document.getElementById(slider.id + "-value");
            document.getElementById(slider.id).addEventListener('change', function () {
                this.setAttribute('value', (this).value);
            });
            slider.oninput = function () {
                sliderInput.value = (this).value;
            };
        });
    }
    static listenInputs(selectorPrefix = String()) {
        let inputs = document.querySelectorAll(selectorPrefix + ".jjslider-value");
        Array.from(inputs).forEach((input) => {
            let slider = document.getElementById(input.id.replace("-value", ""));
            input.oninput = function () {
                slider.value = document.getElementById(input.id).value;
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
        const requestOptions = getRequestOptions();
        modal.showUrl({
            url: url,
            requestOptions: requestOptions
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
class TooltipListener {
    static listen(selectorPrefix) {
        const tooltipTriggerList = document.querySelectorAll(selectorPrefix + '[data-bs-toggle="tooltip"]');
        tooltipTriggerList.forEach(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl, { trigger: 'hover' }));
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
            acceptedFiles: options.allowedTypes,
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
                const items = Array.from(event.clipboardData.files);
                items.forEach((item) => {
                    dropzone.addFile(item);
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
        let dropzone = element.querySelector(".dropzone");
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
        this.url = element.getAttribute("upload-url");
        if (!this.url) {
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
        return this;
    }
    build() {
        const form = document.querySelector("form");
        if (this.url == null) {
            this.url = form.getAttribute("action");
        }
        if (!this.url.includes("?")) {
            this.url += "?";
        }
        else {
            this.url += "&";
        }
        const queryParameters = [...this.queryParameters.entries()];
        for (let i = 0; i < queryParameters.length; i++) {
            const [key, value] = queryParameters[i];
            this.url += `${encodeURIComponent(key)}=${encodeURIComponent(value)}`;
            if (i < queryParameters.length - 1) {
                this.url += "&";
            }
        }
        return this.url;
    }
}
class UrlRedirectModel {
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
            const element = document.getElementById(currentId);
            if (element) {
                const focusableElements = document.querySelectorAll('input:not([disabled]):not([type="hidden"]), select:not([disabled]), button:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"]):not([disabled]):not([hidden])');
                const currentIndex = Array.from(focusableElements).indexOf(element);
                const nextIndex = (currentIndex + 1) % focusableElements.length;
                const nextElement = focusableElements[nextIndex];
                nextElement.focus();
                console.log(nextElement.id);
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
function requestSubmitParentWindow() {
    window.parent.document.forms[0].requestSubmit();
}
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
function onDOMReady(callback) {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', callback);
    }
    else {
        callback();
    }
}
//# sourceMappingURL=jjmasterdata.js.map