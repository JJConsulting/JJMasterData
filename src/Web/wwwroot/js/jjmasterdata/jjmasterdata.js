var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
class ActionManager {
    static executePanelAction(name, action) {
        $("#current-panelAction-" + name).val(action);
        let form = document.querySelector(`form#${name}`);
        if (!form) {
            form = document.forms[0];
        }
        form.requestSubmit();
        return false;
    }
    static executeFormAction(actionName, encryptedActionMap, confirmationMessage) {
        if (confirmationMessage) {
            if (confirm(confirmationMessage)) {
                return false;
            }
        }
        const currentTableActionInput = document.querySelector("#current_tableaction_" + actionName);
        const currentFormActionInput = document.querySelector("#current_formaction_" + actionName);
        let form = document.querySelector("form");
        if (!form) {
            form = document.forms[0];
        }
        currentTableActionInput.value = "";
        currentFormActionInput.value = encryptedActionMap;
        form.submit();
    }
    static executeFormActionAsPopUp(url, title, confirmationMessage) {
        if (confirmationMessage) {
            if (confirm(confirmationMessage)) {
                return false;
            }
        }
        popup.showHtmlFromUrl(title, url, {
            method: "POST",
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: JSON.stringify({})
        }, 1).then(_ => loadJJMasterData());
    }
}
function loadAuditLog(componentName, logId, url = null) {
    $("#sortable_grid a").removeClass("active");
    if (logId != "")
        $("#" + logId).addClass("active");
    document.querySelector('#logId-' + componentName).value = logId;
    if (url == null || url.length == 0) {
        let builder = new UrlBuilder();
        builder.addQueryParameter("t", "ajax");
        url = builder.build();
    }
    fetch(url, {
        method: "POST",
        body: new FormData(document.querySelector("form"))
    }).then(response => response.text()).then(data => {
        document.getElementById("auditlogview-panel-" + componentName).innerHTML = data;
    });
}
function setupCollapsePanel(name) {
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
        $("#sortable_" + context).sortable({
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
    static refreshAction(isPopup = false) {
        SpinnerOverlay.show();
        if (isPopup) {
            window.parent.popup.hide();
            window.parent.document.forms[0].submit();
        }
        else {
            popup.hide();
            document.forms[0].submit();
        }
    }
    static postAction(url) {
        SpinnerOverlay.show();
        $("form:first").attr("action", url).submit();
    }
    static exportElement(id, url, validStr) {
        var values = $("#selectedrows_" + id).val();
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
class DataExportation {
    static startProgressVerificationAtSamePage(componentName) {
        return __awaiter(this, void 0, void 0, function* () {
            DataExportation.setLoadMessage();
            let urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("t", "tableExp");
            urlBuilder.addQueryParameter("gridName", componentName);
            urlBuilder.addQueryParameter("exptype", "checkProgress");
            var isCompleted = false;
            while (!isCompleted) {
                isCompleted = yield DataExportation.checkProgress(urlBuilder.build(), componentName);
                yield sleep(3000);
            }
        });
    }
    static stopProcessAtSamePage(componentName, stopMessage) {
        return __awaiter(this, void 0, void 0, function* () {
            let urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("t", "tableExp");
            urlBuilder.addQueryParameter("gridName", componentName);
            urlBuilder.addQueryParameter("exptype", "stopProcess");
            yield DataExportation.stopExportation(urlBuilder.build(), stopMessage);
        });
    }
    static openExportPopupAtSamePage(componentName) {
        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("t", "tableExp");
        urlBuilder.addQueryParameter("gridName", componentName);
        urlBuilder.addQueryParameter("exptype", "showOptions");
        DataExportation.openExportPopup(urlBuilder.build(), componentName);
    }
    static startExportationAtSamePage(componentName) {
        let urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("t", "tableExp");
        urlBuilder.addQueryParameter("gridName", componentName);
        urlBuilder.addQueryParameter("exptype", "export");
        fetch(urlBuilder.build(), {
            method: "POST",
            body: new FormData(document.querySelector("form"))
        }).then(response => response.text()).then((html) => __awaiter(this, void 0, void 0, function* () {
            const modalBody = "#export_modal_" + componentName + " .modal-body ";
            document.querySelector(modalBody).innerHTML = html;
            loadJJMasterData(null, modalBody);
            yield DataExportation.startProgressVerificationAtSamePage(componentName);
        }));
    }
    static checkProgress(url, componentName) {
        return __awaiter(this, void 0, void 0, function* () {
            showWaitOnPost = false;
            try {
                const response = yield fetch(url);
                const data = yield response.json();
                if (data.FinishedMessage) {
                    showWaitOnPost = true;
                    document.querySelector("#export_modal_" + componentName + " .modal-body").innerHTML = data.FinishedMessage;
                    const linkFile = document.querySelector("#export_link_" + componentName);
                    if (linkFile)
                        linkFile.click();
                    return true;
                }
                else {
                    document.querySelector("#divMsgProcess").style.display = "";
                    document.querySelector(".progress-bar").style.width = data.PercentProcess + "%";
                    document.querySelector(".progress-bar").textContent = data.PercentProcess + "%";
                    document.querySelector("#lblStartDate").textContent = data.StartDate;
                    document.querySelector("#lblResumeLog").textContent = data.Message;
                    return false;
                }
            }
            catch (e) {
                showWaitOnPost = true;
                document.querySelector("#dataexp_spinner_" + componentName).style.display = "none";
                document.querySelector("#export_modal_" + componentName + " .modal-body").innerHTML = e.message;
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
        const target = document.getElementById('exportationSpinner');
        var spinner = new Spinner(options).spin(target);
    }
    static setSettingsHTML(componentName, html) {
        const modalBody = document.querySelector("#export_modal_" + componentName + " .modal-body ");
        modalBody.innerHTML = html;
        loadJJMasterData(null);
        const qtdElement = document.querySelector("#" + componentName + "_totrows");
        if (qtdElement) {
            const totRows = +qtdElement.textContent.replace(/\./g, "");
            if (totRows > 50000) {
                document.querySelector("#warning_exp_" + componentName).style.display = "block";
            }
        }
        if (bootstrapVersion < 5) {
            $("#export_modal_" + componentName).modal();
        }
        else {
            const modal = new bootstrap.Modal(document.querySelector("#export_modal_" + componentName), {});
            modal.show();
        }
    }
    static openExportPopup(url, componentName) {
        fetch(url)
            .then(response => response.text())
            .then(data => {
            this.setSettingsHTML(componentName, data);
        })
            .catch(error => {
            console.log(error);
        });
    }
    static startExportation(startExportationUrl, checkProgressUrl, componentName) {
        const form = document.querySelector("form");
        fetch(startExportationUrl, {
            method: "POST",
            body: new FormData(form)
        })
            .then(response => {
            if (response.ok) {
                return response.text();
            }
            else {
                throw new Error("Request failed with status: " + response.status);
            }
        })
            .then(data => {
            const modalBody = document.querySelector("#export_modal_" + componentName + " .modal-body");
            modalBody.innerHTML = data;
            loadJJMasterData();
            DataExportation.startProgressVerification(checkProgressUrl, componentName);
        })
            .catch(error => {
            console.log(error);
        });
    }
    static stopExportation(url, stopMessage) {
        return __awaiter(this, void 0, void 0, function* () {
            document.querySelector("#divMsgProcess").innerHTML = stopMessage;
            showWaitOnPost = false;
            yield fetch(url);
        });
    }
    static startProgressVerification(url, componentName) {
        return __awaiter(this, void 0, void 0, function* () {
            DataExportation.setLoadMessage();
            var isCompleted = false;
            while (!isCompleted) {
                isCompleted = yield DataExportation.checkProgress(url, componentName);
                yield sleep(3000);
            }
        });
    }
}
class DataImportation {
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
    static checkProgress(componentName) {
        showWaitOnPost = false;
        let checkProgressUrl = document.getElementById("divProcess").getAttribute("check-progress-url");
        let url;
        if (checkProgressUrl) {
            url = checkProgressUrl;
        }
        else {
            let urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("t", "ajaxdataimp");
            urlBuilder.addQueryParameter("current_uploadaction", "process_check");
            urlBuilder.addQueryParameter("objname", componentName);
            url = urlBuilder.build();
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
            const divMsgProcess = document.querySelector("#divMsgProcess");
            if (divMsgProcess) {
                divMsgProcess.style.display = "";
            }
            const progressBar = document.querySelector(".progress-bar");
            if (progressBar) {
                progressBar.style.width = result.PercentProcess + "%";
                progressBar.textContent = result.PercentProcess + "%";
            }
            const lblResumeLog = document.querySelector("#lblResumeLog");
            if (lblResumeLog) {
                lblResumeLog.textContent = result.Message;
            }
            const lblStartDate = document.querySelector("#lblStartDate");
            if (lblStartDate) {
                lblStartDate.textContent = result.StartDate;
            }
            if (result.Insert > 0) {
                document.querySelector("#lblInsert").style.display = "";
                if (result.PercentProcess === 100) {
                    document.querySelector("#lblInsertCount").textContent = result.Insert;
                }
                else {
                    jjutil.animateValue("lblInsertCount", DataImportation.insertCount, result.Insert, 1000);
                }
                DataImportation.insertCount = result.Insert;
            }
            if (result.Update > 0) {
                document.querySelector("#lblUpdate").style.display = "";
                if (result.PercentProcess === 100) {
                    document.querySelector("#lblUpdateCount").textContent = result.Update;
                }
                else {
                    jjutil.animateValue("lblUpdateCount", DataImportation.updateCount, result.Update, 1000);
                }
                DataImportation.updateCount = result.Update;
            }
            if (result.Delete > 0) {
                document.querySelector("#lblDelete").style.display = "";
                if (result.PercentProcess === 100) {
                    document.querySelector("#lblDeleteCount").textContent = result.Delete;
                }
                else {
                    jjutil.animateValue("lblDeleteCount", DataImportation.deleteCount, result.Delete, 1000);
                }
                DataImportation.deleteCount = result.Delete;
            }
            if (result.Ignore > 0) {
                document.querySelector("#lblIgnore").style.display = "";
                if (result.PercentProcess === 100) {
                    document.querySelector("#lblIgnoreCount").textContent = result.Ignore;
                }
                else {
                    jjutil.animateValue("lblIgnoreCount", DataImportation.ignoreCount, result.Ignore, 1000);
                }
                DataImportation.ignoreCount = result.Ignore;
            }
            if (result.Error > 0) {
                document.querySelector("#lblError").style.display = "";
                if (result.PercentProcess === 100) {
                    document.querySelector("#lblErrorCount").textContent = result.Error;
                }
                else {
                    jjutil.animateValue("lblErrorCount", DataImportation.errorCount, result.Error, 1000);
                }
                DataImportation.errorCount = result.Error;
            }
            if (!result.IsProcessing) {
                document.querySelector("#current_uploadaction").value = "process_finished";
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
        let stopProcessUrl = document.getElementById("divProcess").getAttribute("stop-process-url");
        let url;
        if (stopProcessUrl) {
            url = stopProcessUrl;
        }
        else {
            let urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("t", "ajaxdataimp");
            urlBuilder.addQueryParameter("current_uploadaction", "process_check");
            urlBuilder.addQueryParameter("objname", componentName);
            url = urlBuilder.build();
        }
        fetch(url).then(response => response.json()).then(data => {
            if (data.isProcessing === false) {
                document.getElementById("divMsgProcess").innerHTML = stopLabel;
            }
        });
    }
    static addPasteListener() {
        $(document).ready(function () {
            document.addEventListener("paste", (e) => {
                var pastedText = undefined;
                if (window.clipboardData && window.clipboardData.getData) {
                    pastedText = window.clipboardData.getData("Text");
                }
                else if (e.clipboardData && e.clipboardData.getData) {
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
DataImportation.insertCount = 0;
DataImportation.updateCount = 0;
DataImportation.deleteCount = 0;
DataImportation.ignoreCount = 0;
DataImportation.errorCount = 0;
class DataPanel {
    static ReloadAtSamePage(panelname, objid) {
        let url = new UrlBuilder();
        url.addQueryParameter("pnlname", panelname);
        url.addQueryParameter("objname", objid);
        url.addQueryParameter("t", "reloadPanel");
        DataPanel.Reload(url.build(), panelname, objid);
    }
    static Reload(url, componentName, fieldName) {
        const form = document.querySelector("form");
        fetch(url, {
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
            loadJJMasterData();
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
        $(selector)
            .removeClass(FeedbackIcon.successClass)
            .removeClass(FeedbackIcon.warningClass)
            .removeClass(FeedbackIcon.searchClass)
            .removeClass(FeedbackIcon.errorClass);
    }
    static setIcon(selector, iconClass) {
        this.removeAllIcons(selector);
        $(selector).addClass(iconClass);
    }
}
FeedbackIcon.searchClass = "jj-icon-search";
FeedbackIcon.successClass = "jj-icon-success";
FeedbackIcon.warningClass = "jj-icon-warning";
FeedbackIcon.errorClass = "jj-icon-error";
class FormView {
    static postFormValues(objid, enableAjax, loadform) {
        if (enableAjax) {
            const frm = $("form");
            let surl = frm.attr("action");
            if (surl.includes("?"))
                surl += "&t=ajax";
            else
                surl += "?t=ajax";
            surl += "&objname=" + objid;
            $.ajax({
                async: true,
                type: frm.attr("method"),
                url: surl,
                data: frm.serialize(),
                success: function (data) {
                    if (data.substring(2, 18) == "<!--ErrorPage-->") {
                        $("form:first").trigger("submit");
                        return;
                    }
                    $("#jjgridview_" + objid).html(data);
                    if (loadform) {
                        loadJJMasterData();
                    }
                    $("#current_filteraction_" + objid).val("");
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.log(errorThrown);
                    console.log(textStatus);
                    console.log(jqXHR);
                    $("#current_filteraction_" + objid).val("");
                }
            });
        }
        else {
            $("form:first").trigger("submit");
        }
    }
    static selectItem(objid, obj) {
        var values = $("#selectedrows_" + objid).val().toString();
        var valuesList = [];
        if (obj.attr("id") == "jjchk_all")
            return;
        if (values.length > 0) {
            valuesList = values.split(",");
        }
        if (obj.prop("checked")) {
            if ($.inArray(obj.val(), valuesList) < 0)
                valuesList.push(obj.val());
        }
        else {
            valuesList = valuesList.filter(function (item) {
                return item !== obj.val();
            });
        }
        $("#selectedrows_" + objid).val(valuesList);
        var textInfo = "";
        var selectedText = $("#selectedtext_" + objid);
        if (valuesList.length == 0)
            textInfo = selectedText.attr("noSelStr");
        else if (valuesList.length == 1)
            textInfo = selectedText.attr("oneSelStr");
        else
            textInfo = selectedText.attr("paramSelStr").replace("{0}", valuesList.length.toString());
        selectedText.text(textInfo);
    }
    static unSelectAll(objid) {
        $(".jjselect input").not(":disabled").prop("checked", false);
        $("#selectedrows_" + objid).val("");
        var oSelectedtext = $("#selectedtext_" + objid);
        oSelectedtext.text(oSelectedtext.attr("noSelStr"));
    }
    static selectAll(objid) {
        var frm = $("form");
        var surl = frm.attr("action");
        if (surl.includes("?"))
            surl += "&t=selectall";
        else
            surl += "?t=selectall";
        $.ajax({
            async: true,
            type: frm.attr("method"),
            url: surl,
            success: function (data) {
                GridView.selectAllRowsElements(objid, JSON.parse(data).selectedRows);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
                console.log(textStatus);
                console.log(jqXHR);
            }
        });
    }
    static sortFormValues(objid, enableAjax, v) {
        var tableOrder = "#current_tableorder_" + objid;
        if (v + " ASC" == $(tableOrder).val())
            $(tableOrder).val(v + " DESC");
        else
            $(tableOrder).val(v + " ASC");
        $("#current_tableaction_" + objid).val("");
        $("#current_formaction_" + objid).val("");
        this.postFormValues(objid, enableAjax, true);
    }
    static sortItems(objid) {
        var descCommand = "";
        var order = $("#sortable_" + objid).sortable("toArray");
        for (var i = 0; i < order.length; i++) {
            var tipoOrdenacao = $("#" + order[i] + "_order").children("option:selected").val();
            switch (tipoOrdenacao) {
                case "A":
                    descCommand += order[i] + " ASC,";
                    break;
                case "D":
                    descCommand += order[i] + " DESC,";
                    break;
            }
        }
        descCommand = descCommand.substring(0, descCommand.length - 1);
        $("#current_tableorder_" + objid).val(descCommand);
        $("#sort_modal_" + objid).modal('hide');
        $("#current_formaction_" + objid).val("");
        this.refresh(objid, true);
    }
    static paginateGrid(objid, enableAjax, v) {
        $("#current_tablepage_" + objid).val(v);
        $("#current_tableaction_" + objid).val("");
        $("#current_formaction_" + objid).val("");
        this.postFormValues(objid, enableAjax, true);
    }
    static refresh(objid, enableAjax) {
        $("#current_tableaction_" + objid).val("");
        $("#current_tablerow_" + objid).val("");
        $("#current_formaction_" + objid).val("");
        this.postFormValues(objid, enableAjax, true);
    }
    static openSettingsModal(componentName, encryptedActionMap) {
        $("#current_tableaction_" + componentName).val(encryptedActionMap);
        $("#current_tablepage_" + componentName).val("1");
        $("#current_tablerow_" + componentName).val("");
        $("#current_formaction_" + componentName).val("");
        $("form:first").trigger("submit");
    }
    static closeSettingsModal(objid) {
        $("form").trigger("reset");
        $("form :checkbox").change();
        $("#config_modal_" + objid).modal("hide");
    }
    static filter(objid, enableAjax) {
        $("#current_filteraction_" + objid).val("FILTERACTION");
        $("#current_tableaction_" + objid).val("");
        $("#current_tablepage_" + objid).val("1");
        $("#current_formaction_" + objid).val("");
        this.postFormValues(objid, enableAjax, false);
        return false;
    }
    static openSelectElementInsert(componentName, encryptedActionMap) {
        $("#current-panelAction-" + componentName).val("ELEMENTSEL");
        $("#current_selaction_" + componentName).val(encryptedActionMap);
        $("form:first").trigger("submit");
    }
    static clearFilter(objid, enableAjax) {
        var divId = "#gridfilter_" + objid;
        var selector = divId + " input:enabled, " + divId + " select:enabled";
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
                else if (currentObj.hasClass("jjsearchbox")) {
                    currentObj.blur();
                }
                else if (currentObj.hasClass("jjlookup")) {
                    currentObj.blur();
                }
            }
        });
        $("#current_filteraction_" + objid).val("CLEARACTION");
        $("#current_tableaction_" + objid).val("");
        $("#current_formaction_" + objid).val("");
        this.postFormValues(objid, enableAjax, false);
    }
    static gridAction(componentName, encryptedActionMap, confirmMessage) {
        if (confirmMessage) {
            var result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }
        $("#current_tableaction_" + componentName).val(encryptedActionMap);
        $("#current_formaction_" + componentName).val("");
        $("form:first").trigger("submit");
    }
    static redirectFormUrl(objid, criptid, confirmMessage) {
        if (confirmMessage) {
            var result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }
        var frm = $("form");
        var surl = frm.attr("action");
        if (surl.includes("?"))
            surl += "&t=geturlaction&objname=" + objid;
        else
            surl += "?t=geturlaction&objname=" + objid;
        $.ajax({
            async: true,
            type: "POST",
            url: surl,
            data: frm.serialize() + '&criptid=' + criptid,
            success: function (data) {
                if (data.UrlAsPopUp) {
                    popup.show(data.TitlePopUp, data.UrlRedirect);
                }
                else {
                    window.location.href = data.UrlRedirect;
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
                console.log(textStatus);
                console.log(jqXHR);
            }
        });
    }
    static executeSqlCommand(objid, criptid, confirmMessage) {
        if (confirmMessage) {
            var result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }
        $("#current_tableaction_" + objid).val("");
        $("#current_formaction_" + objid).val("");
        $("#current_tablerow_" + objid).val(criptid);
        $("form:first").trigger("submit");
    }
    static setLookup(objid, value) {
        window.parent.popup.hide();
        setTimeout(function () {
            window.parent.$("#id_" + objid).val(value);
            window.parent.$("#" + objid).val(value).change().blur();
        }, 100);
    }
    static executeUrlRedirect(url, ispopup, title, confirmMessage, popupSize = 1) {
        if (confirmMessage) {
            const result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }
        if (ispopup) {
            popup.show(title, url, popupSize);
        }
        else {
            window.location.href = url;
        }
    }
    static showInsertSucess(objid) {
        $("#pnl_insertmsg_" + objid).fadeOut(2000, function () {
            $("#pnl_insert_" + objid).slideDown();
        });
    }
    static deleteFile(objid, filename, promptStr) {
        const result = confirm(promptStr);
        if (!result) {
            return false;
        }
        $("#uploadaction_" + objid).val("DELFILE");
        $("#filename_" + objid).val(filename);
        $("form:first").trigger("submit");
    }
    static downloadFile(objid, filename) {
        $("#uploadaction_" + objid).val("DOWNLOADFILE");
        $("#filename_" + objid).val(filename);
        $("form:first").trigger("submit");
        setTimeout(function () {
            SpinnerOverlay.hide();
            $("#uploadaction_" + objid).val("");
        }, 1500);
    }
    static renameFile(objid, filename, promptStr) {
        var newFileName = prompt(promptStr, filename);
        if (newFileName != null && newFileName != filename) {
            $("#uploadaction_" + objid).val("RENAMEFILE");
            $("#filename_" + objid).val(filename + ";" + newFileName);
            $("form:first").trigger("submit");
        }
    }
    static directDownload(objid, pnlname, filename) {
        SpinnerOverlay.show();
        var url = $("form").attr("action");
        url += url.includes("?") ? "&" : "?";
        url += "jjuploadview_" + pnlname + "=" + objid;
        url += "&downloadfile=" + filename;
        window.location.assign(url);
        setTimeout(function () {
            SpinnerOverlay.hide();
        }, 1500);
    }
    static showExportOptions(objid, exportType) {
        if (exportType == "1") {
            $("#" + objid + "_div_export_orientation").hide();
            $("#" + objid + "_div_export_all").show();
            $("#" + objid + "_div_export_delimiter").hide();
            $("#" + objid + "_div_export_fistline").show();
        }
        else if (exportType == "2") {
            $("#" + objid + "_div_export_orientation").show();
            $("#" + objid + "_div_export_all").hide();
            $("#" + objid + "_div_export_delimiter").hide();
            $("#" + objid + "_div_export_fistline").hide();
        }
        else {
            $("#" + objid + "_div_export_orientation").hide();
            $("#" + objid + "_div_export_all").show();
            $("#" + objid + "_div_export_delimiter").show();
            $("#" + objid + "_div_export_fistline").show();
        }
    }
    static viewLog(objid, id) {
        $("#logId-" + objid).val(id);
        $("form:first").trigger("submit");
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
var _a, _b;
var showWaitOnPost = true;
var bootstrapVersion = 3;
const locale = (_a = document.documentElement.lang) !== null && _a !== void 0 ? _a : 'pt-BR';
const localeCode = (_b = locale.split("-")[0]) !== null && _b !== void 0 ? _b : 'pt';
class GridView {
    static sorting(componentName, url, tableOrder) {
        const tableOrderElement = document.querySelector("#current_tableorder_" + componentName);
        if (tableOrder + " ASC" === tableOrderElement.value)
            tableOrderElement.value = tableOrder + " DESC";
        else
            tableOrderElement.value = tableOrder + " ASC";
        document.querySelector("#current_tableaction_" + componentName).value = "";
        document.querySelector("#current_formaction_" + componentName).value = "";
        GridView.refreshGrid(componentName, url);
    }
    static pagination(componentName, url, currentPage) {
        document.querySelector("#current_tablepage_" + componentName).value = currentPage;
        document.querySelector("#current_tableaction_" + componentName).value = "";
        document.querySelector("#current_formaction_" + componentName).value = "";
        GridView.refreshGrid(componentName, url);
    }
    static filter(componentName, url) {
        document.querySelector("#current_filteraction_" + componentName).value = "FILTERACTION";
        document.querySelector("#current_tableaction_" + componentName).value = "";
        document.querySelector("#current_tablepage_" + componentName).value = "1";
        document.querySelector("#current_formaction_" + componentName).value = "";
        GridView.refreshGrid(componentName, url);
    }
    static refresh(componentName, url) {
        document.querySelector("#current_tableaction_" + componentName).value = "";
        document.querySelector("#current_tablerow_" + componentName).value = "";
        document.querySelector("#current_formaction_" + componentName).value = "";
        GridView.refreshGrid(componentName, url);
    }
    static selectAllRows(componentName, url) {
        fetch(url, { method: "POST" })
            .then(response => response.json())
            .then(data => GridView.selectAllRowsElements(componentName, data.selectedRows));
    }
    static selectAllRowsElements(componentName, rows) {
        const values = rows.split(",");
        const checkboxes = document.querySelectorAll(".jjselect input:not(:disabled)");
        checkboxes.forEach(checkbox => checkbox.checked = true);
        const selectedRowsInput = document.getElementById("selectedrows_" + componentName);
        selectedRowsInput.value = values.join(",");
        const selectedText = document.getElementById("selectedtext_" + componentName);
        selectedText.textContent = selectedText.getAttribute("paramSelStr").replace("{0}", values.length.toString());
    }
    static refreshGrid(componentName, url) {
        const form = document.querySelector("form");
        let urlBuilder = new UrlBuilder(url);
        urlBuilder.addQueryParameter("componentName", componentName);
        SpinnerOverlay.show();
        fetch(urlBuilder.build(), {
            method: form.method,
            body: new FormData(form)
        })
            .then(response => response.text())
            .then(data => {
            document.querySelector("#jjgridview_" + componentName).innerHTML = data;
            loadJJMasterData();
            document.querySelector("#current_filteraction_" + componentName).value = "";
            SpinnerOverlay.hide();
        })
            .catch(error => {
            console.log(error);
            document.querySelector("#current_filteraction_" + componentName).value = "";
        });
    }
}
$(function () {
    bootstrapVersion = $.fn.tooltip.Constructor.VERSION.charAt(0);
    loadJJMasterData("load", null);
});
class JJSortable {
    static setup() {
        $(".jjsortable").sortable({
            helper: function (e, tr) {
                var $originals = tr.children();
                var $helper = tr.clone();
                $helper.children().each(function (index) {
                    $(this).width($originals.eq(index).width());
                });
                return $helper;
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
function loadJJMasterData(event, prefixSelector) {
    if (prefixSelector === undefined || prefixSelector === null) {
        prefixSelector = "";
    }
    $(prefixSelector + ".selectpicker").selectpicker({
        iconBase: 'fa'
    });
    $(prefixSelector + "input[type=checkbox][data-toggle^=toggle]").bootstrapToggle();
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
    TextArea.setup();
    SearchBox.setup();
    Lookup.setup();
    JJSortable.setup();
    UploadArea.setup();
    TabNav.setup();
    Slider.observeSliders();
    Slider.observeInputs();
    $(document).on({
        ajaxSend: function (event, jqXHR, settings) {
            if (settings.url != null &&
                settings.url.indexOf("t=jjsearchbox") !== -1) {
                return null;
            }
            if (showWaitOnPost) {
                SpinnerOverlay.show();
            }
        },
        ajaxStop: function () { SpinnerOverlay.hide(); }
    });
    $("form").on("submit", function () {
        let isValid;
        try {
            isValid = $("form").valid();
        }
        catch (_a) {
            isValid = true;
        }
        if (isValid && showWaitOnPost) {
            setTimeout(function () { SpinnerOverlay.show(); }, 1);
        }
    });
}
class Lookup {
    static setup() {
        $("input.jjlookup").each(function () {
            let lookupInput = $(this);
            let lookupId = lookupInput.attr("id");
            let panelName = lookupInput.attr("pnlname");
            let popupTitle = lookupInput.attr("popuptitle");
            let lookupUrl = lookupInput.attr("lookup-url");
            let popupSize = +lookupInput.attr("popupsize");
            let form = $("form");
            let url = form.attr("action");
            if (url.includes("?"))
                url += "&";
            else
                url += "?";
            url += "jjlookup_";
            url += panelName + "=" + lookupId;
            const jjLookupSelector = "#" + lookupId + "";
            const jjHiddenLookupSelector = "#id_" + lookupId + "";
            $("#btn_" + lookupId).on("click", function () {
                popup.show(popupTitle, lookupUrl, popupSize);
            });
            function setHiddenLookup() {
                $("#id_" + lookupId).val(lookupInput.val());
            }
            lookupInput.one("focus", function () {
                lookupInput.val($("#id_" + lookupId).val()).select();
            });
            lookupInput.one("change", function () {
                $("#id_" + lookupId).val(lookupInput.val());
            });
            lookupInput.one("blur", function () {
                showWaitOnPost = false;
                setHiddenLookup();
                FeedbackIcon.removeAllIcons(jjLookupSelector);
                lookupInput.removeAttr("readonly");
                if (lookupInput.val() == "") {
                    return;
                }
                lookupInput.addClass("loading-circle");
                const ajaxUrl = url + "&lkaction=ajax&lkid=" + lookupInput.val();
                $.ajax({
                    type: 'POST',
                    data: form.serialize(),
                    dataType: "json",
                    cache: false,
                    async: true,
                    url: ajaxUrl,
                    success: function (data) {
                        showWaitOnPost = true;
                        lookupInput.removeClass("loading-circle");
                        if (data.description == "") {
                            FeedbackIcon.setIcon(jjLookupSelector, FeedbackIcon.warningClass);
                        }
                        else {
                            const lookupHiddenInputElement = document.getElementById("id_" + lookupId);
                            const lookupInputElement = document.getElementById(lookupId);
                            FeedbackIcon.setIcon(jjLookupSelector, FeedbackIcon.successClass);
                            lookupInputElement.value = data.description;
                            lookupHiddenInputElement.value = data.id;
                            DataPanel.ReloadAtSamePage(panelName, lookupId);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        showWaitOnPost = true;
                        lookupInput.removeClass("loading-circle");
                        FeedbackIcon.setIcon(jjLookupSelector, FeedbackIcon.errorClass);
                        console.log(errorThrown);
                        console.log(textStatus);
                        console.log(jqXHR);
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
class Modal {
    constructor() {
        this.modalId = "popup-modal";
        this.modalTitleId = "popup-modal-title";
    }
    setTitle(title) {
        document.getElementById(this.modalTitleId).innerHTML = title;
    }
    showModal(isIframe = true) {
        if (bootstrapVersion < 5) {
            $("#" + this.modalId).modal();
        }
        else {
            const modal = new bootstrap.Modal(document.getElementById(this.modalId), {});
            modal.show();
        }
        if (isIframe) {
            SpinnerOverlay.show();
            $("iframe").on("load", function () {
                SpinnerOverlay.hide();
            });
        }
    }
    loadHtml(content, size, isIframe = true) {
        const modalIdSelector = `#${this.modalId}`;
        if ($(modalIdSelector).length) {
            $(modalIdSelector).remove();
        }
        let width;
        let height;
        if (size === undefined) {
            size = "1";
        }
        size = parseInt(size);
        let modalDialogDiv;
        switch (size) {
            case 1:
                width = "98%";
                height = "92%";
                modalDialogDiv = "<div class=\"modal-dialog\" style=\"margin:0.7em;left:0px;right:0px;top:0px;bottom:0px; position:fixed;width:auto;\">\r\n";
                break;
            case 2:
                width = "auto";
                height = "95%";
                modalDialogDiv = "<div class=\"modal-dialog\" style=\"position: auto; height: 95vh;width:auto;\">\r\n";
                break;
            case 3:
                width = "50%";
                height = "65%";
                modalDialogDiv = "<div class=\"modal-dialog\" style=\"position: auto; height: 65vh;width:50%\">\r\n";
                break;
            default:
                width = "65%";
                height = "80%";
                modalDialogDiv = "<div class=\"modal-dialog\" style=\"position: auto; height: 80vh;width:65%\">\r\n";
                break;
        }
        let modalDialogCss = `
@media (min-width: 576px) {
  .modal-dialog { max-width: none; }
}

.modal-dialog {
  width: ${width};
  height: ${height};
  padding: 0;
}

.modal-content {
  height: 100%;
}
`;
        let html = "";
        html += `<div id=\"${this.modalId}\" tabindex=\"-1\" class=\"modal fade\" role=\"dialog\">\r\n`;
        if (bootstrapVersion == 3) {
            html += modalDialogDiv;
        }
        else {
            $('head').append(`<style type="text/css">${modalDialogCss}</style>`);
            html += "<div class=\"modal-dialog\">";
        }
        if (bootstrapVersion != 3) {
            html += `    <div class="modal-content">\r\n`;
        }
        else {
            html += `    <div class="modal-content" style="height:100%;width:auto;">\r\n`;
        }
        html += "      <div class=\"modal-header\">\r\n";
        if (bootstrapVersion == 3) {
            html += "        <button type=\"button\" class=\"close\" data-dismiss=\"modal\">&times;</button>\r\n";
            html += `       <h4 id=\"${this.modalTitleId}\" class=\"modal-title\"></h4>\r\n`;
        }
        else {
            html += `        <h4 id=\"${this.modalTitleId}\" class=\"modal-title\"></h4>\r\n`;
            if (bootstrapVersion >= 5) {
                html += "        <button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"modal\"></button>\r\n";
            }
            else {
                html += "        <button type=\"button\" class=\"close\" data-dismiss=\"modal\">&times;</button>\r\n";
            }
        }
        html += "      </div>\r\n";
        html += "      <div class=\"modal-body\"  style=\"height:90%;width:auto;\">\r\n";
        if (isIframe) {
            html += "         <iframe style=\"border: 0px;\" ";
            html += " src='";
            html += content;
            html += "' width='100%' height='97%'>Waiting...</iframe>";
        }
        else {
            html += content;
        }
        html += "      </div>\r\n";
        html += "    </div>\r\n";
        html += "  </div>\r\n";
        html += "</div>\r\n";
        $(html).appendTo($("body"));
    }
    show(title, url, size = 1) {
        this.loadHtml(url, size);
        this.setTitle(title);
        this.showModal();
    }
    showHtml(title, html, size = 1) {
        this.loadHtml(html, size, false);
        this.setTitle(title);
        this.showModal(false);
    }
    showHtmlFromUrl(title, url, options = null, size = 1) {
        return __awaiter(this, void 0, void 0, function* () {
            SpinnerOverlay.show();
            yield fetch(url, options)
                .then(response => response.text())
                .then(html => {
                this.showHtml(title, html, size);
                SpinnerOverlay.hide();
            })
                .catch(error => {
                console.error('Error fetching HTML from URL:', error);
            });
        });
    }
    hide() {
        $("#" + this.modalId).modal("hide");
    }
    modal() {
        return $("#" + this.modalId);
    }
}
var popup = function () {
    if (!(this instanceof Modal)) {
        return new Modal();
    }
}();
class SearchBox {
    static setup() {
        $("input.jjsearchbox").each(function () {
            const componentName = $(this).attr("jjid");
            let urltypehead = $(this).attr("urltypehead");
            let triggerlength = $(this).attr("triggerlength");
            let numberofitems = $(this).attr("numberofitems");
            let scrollbar = Boolean($(this).attr("scrollbar"));
            let showimagelegend = Boolean($(this).attr("showimagelegend"));
            if (triggerlength == null)
                triggerlength = "1";
            if (numberofitems == null)
                numberofitems = "10";
            if (scrollbar == null)
                scrollbar = false;
            if (showimagelegend == null)
                showimagelegend = false;
            const frm = $("form");
            if (!urltypehead.includes("GetItems")) {
                let url = frm.attr("action");
                if (url.includes("?"))
                    url += "&";
                else
                    url += "?";
                urltypehead = url + urltypehead;
            }
            const jjSearchBoxSelector = "#" + componentName + "_text";
            const jjSearchBoxHiddenSelector = "#" + componentName;
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
                    url: urltypehead,
                    method: "POST",
                    loadingClass: "loading-circle",
                    triggerLength: triggerlength,
                    preDispatch: function () {
                        $(jjSearchBoxHiddenSelector).val("");
                        FeedbackIcon.setIcon(jjSearchBoxSelector, "");
                        return frm.serializeArray();
                    },
                },
                onSelect: function (item) {
                    $(jjSearchBoxHiddenSelector).val(item.value);
                    if (item.value != "") {
                        FeedbackIcon.setIcon(jjSearchBoxSelector, FeedbackIcon.successClass);
                    }
                },
                displayField: "name",
                valueField: "id",
                triggerLength: triggerlength,
                items: numberofitems,
                scrollBar: scrollbar,
                item: '<li class="dropdown-item"><a href="#"></a></li>',
                highlighter: function (item) {
                    const query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, "\\$&");
                    let textSel;
                    if (showimagelegend) {
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
class Slider {
    static observeSliders() {
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
    static observeInputs() {
        let inputs = document.getElementsByClassName("jjslider-value");
        Array.from(inputs).forEach((input) => {
            let slider = document.getElementById(input.id.replace("-value", ""));
            input.oninput = function () {
                slider.value = $("#" + input.id).val();
            };
        });
    }
}
class SpinnerOverlay {
    static loadHtml() {
        if (!$(this.spinnerOverlayId).length) {
            const html = `
                <div id="spinner-overlay">
                    <div class="ajaxImage"></div>
                    <div class="ajaxMessage">Loading...</div>
                </div>
            `;
            $(html).insertAfter($("body"));
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
            this.spinner = new Spinner(options).spin();
            if (this.spinner.el) {
                $(this.spinner.el).insertAfter($("#spinner-overlay .ajaxImage"));
            }
        }
    }
    static show() {
        this.loadHtml();
        document.querySelector(this.spinnerOverlayId).style.display = "";
    }
    static hide() {
        document.querySelector(this.spinnerOverlayId).style.display = "none";
    }
}
SpinnerOverlay.spinnerOverlayId = "#spinner-overlay";
class TabNav {
    static setup() {
        $("a.jj-tab-link").on("shown.bs.tab", function (e) {
            var link = $(e.target);
            $("#" + link.attr("jj-objectid")).val(link.attr("jj-tabindex"));
        });
    }
}
class TextArea {
    static setup() {
        $("textarea").keydown(function () {
            var oTextArea = $(this);
            var iMaxChar = oTextArea.attr("maxlength");
            var strValid = oTextArea.attr("strvalid2");
            var strChars = oTextArea.attr("strchars");
            if (isNaN(iMaxChar))
                iMaxChar = oTextArea.attr("jjmaxlength");
            if (isNaN(iMaxChar))
                return;
            if (isNaN(strValid))
                strValid = "Maximum limit of {0} characters!";
            if (isNaN(strChars))
                strChars = "({0} characters remaining)";
            if (!isNaN(iMaxChar)) {
                var nId = oTextArea.attr("id");
                var iTypedChar = oTextArea.val().toString().length;
                if (iTypedChar > iMaxChar) {
                    alert(strValid.replace("{0}", iMaxChar));
                }
                strChars = strChars.replace("{0}", (iMaxChar - oTextArea.val().toString().length));
                strChars += "&nbsp;";
                if ($("#spansize_" + nId).length) {
                    $("#spansize_" + nId).html(strChars);
                }
                else {
                    $("<span id='spansize_" + nId + "' class='small' style='float: right'>" + strChars + "</span>").insertBefore(oTextArea);
                }
            }
        });
    }
}
class FileUploadOptions {
    constructor(componentName, url, form, allowMultiple, maxFileSize, allowDragDrop, showFileSize, allowedTypes, dragDropLabel, autoSubmit) {
        this.componentName = componentName;
        this.url = url;
        this.form = form;
        this.allowMultiple = allowMultiple;
        this.maxFileSize = maxFileSize;
        this.allowDragDrop = allowDragDrop;
        this.showFileSize = showFileSize;
        this.allowedTypes = allowedTypes;
        this.dragDropLabel = dragDropLabel;
        this.autoSubmit = autoSubmit;
    }
}
class UploadArea {
    static uploadFile(options) {
        const selector = "#" + options.componentName;
        $(selector).uploadFile({
            url: options.url,
            formData: $(options.form).serializeArray(),
            fileName: "file",
            multiple: options.allowMultiple,
            maxFileSize: options.maxFileSize,
            maxFileCount: 1000,
            dragDrop: options.allowDragDrop,
            showFileSize: options.showFileSize,
            dragdropWidth: ($(selector).width() - 10),
            statusBarWidth: ($(selector).width() - 10),
            autoSubmit: true,
            uploadButtonClass: "btn btn-primary",
            allowedTypes: options.allowedTypes,
            acceptFiles: options.allowedTypes !== "*" ? "." + options.allowedTypes.replace(" ", "").replace(",", ",.") : "*",
            uploadStr: $(selector).attr("uploadStr"),
            dragDropStr: "<span>&nbsp;<b>" + options.dragDropLabel + "</b></span>",
            doneStr: $(selector).attr("doneStr"),
            cancelStr: $(selector).attr("cancelStr"),
            abortStr: $(selector).attr("abortStr"),
            extErrorStr: $(selector).attr("extErrorStr"),
            sizeErrorStr: $(selector).attr("sizeErrorStr"),
            customErrorKeyStr: "jquery-upload-file-error",
            returnType: "json",
            onSubmit: function () {
                showWaitOnPost = false;
            },
            onSuccess: function (files, data, xhr, pd) {
                var message = data["jquery-upload-file-message"];
                if (message && message != "") {
                    var div = $("<div class='ajax-file-upload-filename'></div>");
                    $("<span class='fa fa-check-circle text-success'></span>").appendTo(div);
                    $("<span>&nbsp;" + message + "</span>").appendTo(div);
                    div.appendTo(pd.statusbar[0]);
                }
            },
            afterUploadAll: function (element) {
                if (options.autoSubmit && element.selectedFiles > 0) {
                    $("#uploadaction_" + options.componentName).val("afteruploadall");
                }
                loadJJMasterData();
            },
        });
    }
    static handleCopyPaste(componentName) {
        window.addEventListener("paste", (e) => {
            var files = e.clipboardData.files;
            if (files.length === 1) {
                var file = files[0];
                if (file.type.indexOf("image") !== -1) {
                    document.querySelector("#btnDoUpload_" + componentName).addEventListener("click", () => {
                        document.querySelector("#preview_modal_" + componentName).classList.add("hide");
                        var filename = document.querySelector("#preview_filename_" + componentName).value || "image";
                        filename += ".png";
                        const dt = new DataTransfer();
                        const myNewFile = new File([file], filename, { type: file.type });
                        dt.items.add(myNewFile);
                        document.querySelector("#" + componentName + " input[type='file']").files = dt.files;
                        document.querySelector("#" + componentName + " input[type='file']").dispatchEvent(new Event("change"));
                        return;
                    });
                    var reader = new FileReader();
                    reader.onload = function (event) {
                        document.querySelector("#pastedimage_" + componentName).src = event.target.result.toString();
                        var filename = file.name.replace(/\.[^/.]+$/, "");
                        document.querySelector("#preview_filename_" + componentName).value = filename;
                        document.querySelector("#preview_modal_" + componentName).classList.remove("hide");
                    };
                    reader.readAsDataURL(file);
                    return;
                }
            }
            document.querySelector("#" + componentName + " input[type='file']").files = files;
            document.querySelector("#" + componentName + " input[type='file']").dispatchEvent(new Event("change"));
        });
    }
    static setup() {
        document.querySelectorAll("div.fileUpload").forEach((element) => {
            let componentName = element.getAttribute("id");
            let multiple = element.getAttribute("jjmultiple") === "true";
            let autoSubmit = element.getAttribute("autoSubmit") === "true";
            let maxFileSize = element.getAttribute("maxFileSize");
            let dragDrop = element.getAttribute("dragDrop");
            let copyPaste = element.getAttribute("copyPaste");
            let showFileSize = element.getAttribute("showFileSize");
            let allowedTypes = element.getAttribute("allowedTypes");
            let dragDropStr = "<span>&nbsp;<b>" + element.getAttribute("dragDropStr") + "</b></span>";
            let frm = document.querySelector("form");
            let url;
            if (element.getAttribute("url") != null) {
                url = element.getAttribute("url");
            }
            else {
                let urlBuilder = new UrlBuilder();
                urlBuilder.addQueryParameter("t", "jjupload");
                urlBuilder.addQueryParameter("objname", componentName);
                url = urlBuilder.build();
            }
            const fileUploadOptions = new FileUploadOptions(componentName, url, frm, multiple, maxFileSize, dragDrop, showFileSize, allowedTypes, dragDropStr, autoSubmit);
            this.uploadFile(fileUploadOptions);
            window.addEventListener("resize", () => {
                document.querySelector("#" + componentName + " .ajax-upload-dragdrop").style.width =
                    document.querySelector("#" + componentName).clientWidth - 30 + "px";
            });
            if (copyPaste === "true") {
                this.handleCopyPaste(componentName);
            }
        });
    }
}
class UploadView {
    static open(componentName, title, values, url = null) {
        const panelName = $("#v_" + componentName).attr("pnlname");
        if (url == null) {
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("jjuploadview_" + panelName, componentName);
            urlBuilder.addQueryParameter("uploadViewParams", values);
            url = urlBuilder.build();
        }
        const popup = new Modal();
        popup.modalId = componentName + "-popup";
        popup.modalTitleId = componentName + "-popup-title";
        if (url == null || url.length == 0) {
            popup.show(title, url, 1);
        }
        else {
            popup.showHtmlFromUrl(title, url, null, 1).then(_ => {
                loadJJMasterData();
            });
        }
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
//# sourceMappingURL=/Scripts/jjmasterdata.js.map