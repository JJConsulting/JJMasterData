document.addEventListener("DOMContentLoaded", function () {
    bootstrapVersion = $.fn.tooltip.Constructor.VERSION.charAt(0);
    $.ajaxSetup({
        xhrFields: {
            withCredentials: true
        }
    });
    jjloadform("load");
});
function setupCollapsePanel(name) {
    let nameSelector = "#" + name;
    let collapseSelector = '#collapse_mode_' + name;
    document.addEventListener("DOMContentLoaded", function () {
        $(nameSelector).on('hidden.bs.collapse', function () {
            $(collapseSelector).val("0");
        });
        $(nameSelector).on('show.bs.collapse', function () {
            $(collapseSelector).val("1");
        });
    });
}
class JJDataExp {
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
        var target = document.getElementById('impSpin');
        var spinner = new Spinner(options).spin(target);
    }
    static checkProcess(objname, intervalId) {
        showWaitOnPost = false;
        var form = $("form");
        var formUrl = form.attr("action");
        if (formUrl.includes("?"))
            formUrl += "&t=tableexp";
        else
            formUrl += "?t=tableexp";
        formUrl += "&gridName=" + objname;
        formUrl += "&exptype=checkProcess";
        fetch(formUrl)
            .then(response => response.json())
            .then(function (data) {
            if (data.FinishedMessage) {
                clearInterval(intervalId);
                showWaitOnPost = true;
                $("#export_modal_" + objname + " .modal-body").html(data.FinishedMessage);
                $("#dataexp_spinner_" + objname).hide();
                const linkFile = $("#export_link_" + objname)[0];
                if (linkFile)
                    linkFile.click();
            }
            else {
                $("#divMsgProcess").css("display", "");
                $(".progress-bar").css("width", data.PercentProcess + "%").text(data.PercentProcess + "%");
                $("#lblStartDate").text(data.StartDate);
                $("#lblResumeLog").text(data.Message);
            }
        });
    }
    static startProcess(objname) {
        JJDataExp.setLoadMessage();
        let intervalId = setInterval(function () {
            JJDataExp.checkProcess(objname, intervalId);
        }, (3 * 1000));
    }
    static stopProcess(objid, stopStr) {
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
        fetch(surl);
    }
    static openExportUI(objid) {
        var frm = $("form");
        var surl = frm.attr("action");
        if (surl.includes("?"))
            surl += "&t=tableexp";
        else
            surl += "?t=tableexp";
        surl += "&gridName=" + objid;
        surl += "&exptype=showoptions";
        $.ajax({
            async: true,
            type: frm.attr("method"),
            url: surl,
            success: function (data) {
                var modalBody = "#export_modal_" + objid + " .modal-body ";
                $(modalBody).html(data);
                jjloadform(null, modalBody);
                var qtdElement = $("#" + objid + "_totrows");
                if (qtdElement.length > 0) {
                    var totRows = +qtdElement.text().replace(".", "").replace(".", "").replace(".", "").replace(".", "");
                    if (totRows > 50000)
                        $("#warning_exp_" + objid).show();
                }
                if (bootstrapVersion < 5) {
                    $("#export_modal_" + objid).modal();
                }
                else {
                    const modal = new bootstrap.Modal("#export_modal_" + objid, {});
                    modal.show();
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
                console.log(textStatus);
                console.log(jqXHR);
            }
        });
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
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
                console.log(textStatus);
                console.log(jqXHR);
            }
        });
    }
}
class JJDataImp {
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
    static checkProcess(objname) {
        showWaitOnPost = false;
        const form = $("form");
        let url = form.attr("action");
        if (url.includes("?"))
            url += "&t=ajaxdataimp&current_uploadaction=process_check&objname=" + objname;
        else
            url += "?t=ajaxdataimp&current_uploadaction=process_check&objname=" + objname;
        $.ajax({
            url: url,
            dataType: "json",
            cache: false,
            success: function (result) {
                $("#divMsgProcess").css("display", "");
                $(".progress-bar").css("width", result.PercentProcess + "%")
                    .text(result.PercentProcess + "%");
                $("#lblResumeLog").text(result.Message);
                $("#lblStartDate").text(result.StartDate);
                if (result.Insert > 0) {
                    $("#lblInsert").css("display", "");
                    if (result.PercentProcess == 100)
                        $("#lblInsertCount").text(result.Insert);
                    else
                        jjutil.animateValue("lblInsertCount", JJDataImp.insertCount, result.Insert, 1000);
                    JJDataImp.insertCount = result.Insert;
                }
                if (result.Update > 0) {
                    $("#lblUpdate").css("display", "");
                    if (result.PercentProcess == 100)
                        $("#lblUpdateCount").text(result.Update);
                    else
                        jjutil.animateValue("lblUpdateCount", JJDataImp.updateCount, result.Update, 1000);
                    JJDataImp.updateCount = result.Update;
                }
                if (result.Delete > 0) {
                    $("#lblDelete").css("display", "");
                    if (result.PercentProcess == 100)
                        $("#lblDeleteCount").text(result.Delete);
                    else
                        jjutil.animateValue("lblDeleteCount", JJDataImp.deleteCount, result.Delete, 1000);
                    JJDataImp.deleteCount = result.Delete;
                }
                if (result.Ignore > 0) {
                    $("#lblIgnore").css("display", "");
                    if (result.PercentProcess == 100)
                        $("#lblIgnoreCount").text(result.Ignore);
                    else
                        jjutil.animateValue("lblIgnoreCount", JJDataImp.ignoreCount, result.Ignore, 1000);
                    JJDataImp.ignoreCount = result.Ignore;
                }
                if (result.Error > 0) {
                    $("#lblError").css("display", "");
                    if (result.PercentProcess == 100)
                        $("#lblErrorCount").text(result.Error);
                    else
                        jjutil.animateValue("lblErrorCount", JJDataImp.errorCount, result.Error, 1000);
                    JJDataImp.errorCount = result.Error;
                }
                if (!result.IsProcessing) {
                    $("#current_uploadaction").val("process_finished");
                    setTimeout(function () {
                        $("form:first").submit();
                    }, 1000);
                }
            }
        });
    }
    static startProcess(objname) {
        $(document).ready(function () {
            JJDataImp.setLoadMessage();
            setInterval(function () {
                JJDataImp.checkProcess(objname);
            }, 3000);
        });
    }
    static stopProcess(objname, stopStr) {
        $("#divMsgProcess").html(stopStr);
        showWaitOnPost = false;
        const form = $("form");
        let url = form.attr("action");
        if (url.includes("?"))
            url += "&t=ajaxdataimp&current_uploadaction=process_stop&objname=" + objname;
        else
            url += "?t=ajaxdataimp&current_uploadaction=process_stop&objname=" + objname;
        $.ajax({
            url: url,
            dataType: "json",
            cache: false
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
                    $("form:first").submit();
                }
                return false;
            });
        });
    }
}
JJDataImp.insertCount = 0;
JJDataImp.updateCount = 0;
JJDataImp.deleteCount = 0;
JJDataImp.ignoreCount = 0;
JJDataImp.errorCount = 0;
class JJDataPanel {
    static doReload(panelname, objid) {
        const frm = $("form");
        let surl = frm.attr("action");
        surl += surl.includes("?") ? "&" : "?";
        surl += "t=reloadpainel";
        surl += "&pnlname=" + panelname;
        surl += "&objname=" + objid;
        $.ajax({
            async: true,
            type: frm.attr("method"),
            url: surl,
            data: frm.serialize(),
            success: function (data) {
                $("#" + panelname).html(data);
                jjloadform();
                jjutil.gotoNextFocus(objid);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
                console.log(textStatus);
                console.log(jqXHR);
            }
        });
    }
}
var jjdictionary = (function () {
    return {
        deleteAction: function (actionName, url, questionStr) {
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
                        messageWait.hide();
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
        },
        doActionSortable: function (context, url, errorStr) {
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
                            messageWait.hide();
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
        },
        setDisableAction: function (isDisable, url, errorStr) {
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
                    messageWait.hide();
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
        },
        refreshAction: function (isPopup = false) {
            if (!isPopup)
                popup.hide();
            else
                window.parent.popup.hide();
            messageWait.show();
            document.forms[0].submit();
        },
        postAction: function (url) {
            messageWait.show();
            $("form:first").attr("action", url).submit();
        },
        exportElement: function (id, url, validStr) {
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
                messageWait.hide();
            }, 2000);
            return true;
        }
    };
})();
class JJFeedbackIcon {
    static removeAllIcons(selector) {
        $(selector)
            .removeClass(JJFeedbackIcon.successClass)
            .removeClass(JJFeedbackIcon.warningClass)
            .removeClass(JJFeedbackIcon.searchClass)
            .removeClass(JJFeedbackIcon.errorClass);
    }
    static setIcon(selector, iconClass) {
        this.removeAllIcons(selector);
        $(selector).addClass(iconClass);
    }
}
JJFeedbackIcon.searchClass = "jj-icon-search";
JJFeedbackIcon.successClass = "jj-icon-success";
JJFeedbackIcon.warningClass = "jj-icon-warning";
JJFeedbackIcon.errorClass = "jj-icon-error";
function jjloadform(event, prefixSelector) {
    if (prefixSelector === undefined) {
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
        altFormat: localeCode === "pt" ? "d/m/Y H:i" : "m/d/Y H:i",
        dateFormat: localeCode === "pt" ? "d-m-Y H:i" : "m-d-Y H:i",
        onOpen: function (selectedDates, dateStr, instance) {
            instance.setDate(Date.now());
        },
        locale: localeCode
    });
    $(prefixSelector + ".jjform-date").flatpickr({
        enableTime: false,
        wrap: true,
        allowInput: true,
        altInput: false,
        altFormat: localeCode === "pt" ? "d/m/Y" : "m/d/Y",
        dateFormat: localeCode === "pt" ? "d-m-Y" : "m-d-Y",
        onOpen: function (selectedDates, dateStr, instance) {
            instance.setDate(Date.now());
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
        altFormat: "H:i",
        time_24hr: true,
        onOpen: function (selectedDates, dateStr, instance) {
            instance.setDate(Date.now());
        },
        locale: localeCode
    });
    $(prefixSelector + ".jjdecimal").each(function () {
        let decimalPlaces = $(this).attr("jjdecimalplaces");
        if (decimalPlaces == null)
            decimalPlaces = "2";
        if (localeCode === 'pt')
            $(this).number(true, decimalPlaces, ",", ".");
        else
            $(this).number(true, decimalPlaces);
    });
    $(prefixSelector + "[data-toggle='tooltip'], " + prefixSelector + "[data-bs-toggle='tooltip']").tooltip({
        container: "body",
        trigger: "hover"
    });
    JJTextArea.setup();
    JJSearchBox.setup();
    JJLookup.setup();
    JJSortable.setup();
    JJUpload.setup();
    JJTabNav.setup();
    JJSlider.observeSliders();
    JJSlider.observeInputs();
    messageWait.hide();
    $(document).on({
        ajaxSend: function (event, jqXHR, settings) {
            if (settings.url != null &&
                settings.url.indexOf("t=jjsearchbox") !== -1) {
                return null;
            }
            if (showWaitOnPost) {
                messageWait.show();
            }
        },
        ajaxStop: function () { messageWait.hide(); }
    });
    $("form").on("submit", function () {
        if (showWaitOnPost) {
            setTimeout(function () { messageWait.show(); }, 1);
        }
    });
}
class JJLookup {
    static setup() {
        $("input.jjlookup").each(function () {
            let lookupInput = $(this);
            let lookupId = lookupInput.attr("id");
            let panelName = lookupInput.attr("pnlname");
            let popupTitle = lookupInput.attr("popuptitle");
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
                let ajaxUrl = url + "&lkaction=geturl&lkid=" + lookupInput.val();
                $.ajax({
                    type: 'POST',
                    data: form.serialize(),
                    dataType: "json",
                    cache: false,
                    url: ajaxUrl,
                    success: function (data) {
                        popup.show(popupTitle, data.url, popupSize);
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        console.log(errorThrown);
                        console.log(textStatus);
                        console.log(jqXHR);
                    }
                });
                return false;
            });
            lookupInput.on("focus", function () {
                lookupInput.val($("#id_" + lookupId).val())
                    .removeAttr("readonly")
                    .select();
            });
            lookupInput.on("change", function () {
                $("#id_" + lookupId).val(lookupInput.val());
            });
            lookupInput.on("blur", function () {
                showWaitOnPost = false;
                $(jjHiddenLookupSelector).val(lookupInput.val());
                JJFeedbackIcon.removeAllIcons(jjLookupSelector);
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
                    url: ajaxUrl,
                    success: function (data) {
                        showWaitOnPost = true;
                        lookupInput.removeClass("loading-circle");
                        if (data.description == "") {
                            JJFeedbackIcon.setIcon(jjLookupSelector, JJFeedbackIcon.warningClass);
                            lookupInput.removeAttr("readonly");
                        }
                        else {
                            JJFeedbackIcon.setIcon(jjLookupSelector, JJFeedbackIcon.successClass);
                            lookupInput.attr("readonly", "readonly").val(data.description);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        showWaitOnPost = true;
                        lookupInput.removeClass("loading-circle");
                        JJFeedbackIcon.setIcon(jjLookupSelector, JJFeedbackIcon.errorClass);
                        console.log(errorThrown);
                        console.log(textStatus);
                        console.log(jqXHR);
                    }
                });
            });
        });
    }
}
class JJSearchBox {
    static setup() {
        $("input.jjsearchbox").each(function () {
            const objid = $(this).attr("jjid");
            const pnlname = $(this).attr("pnlname");
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
            let urltypehead = frm.attr("action");
            if (urltypehead.includes("?"))
                urltypehead += "&";
            else
                urltypehead += "?";
            urltypehead += "t=jjsearchbox";
            urltypehead += "&objname=" + objid;
            urltypehead += "&pnlname=" + pnlname;
            const jjSearchBoxSelector = "#" + objid + "_text";
            const jjSearchBoxHiddenSelector = "#" + objid;
            $(this).blur(function () {
                if ($(this).val() == "") {
                    JJFeedbackIcon.setIcon(jjSearchBoxSelector, JJFeedbackIcon.searchClass);
                    $(jjSearchBoxHiddenSelector).val("");
                }
                else if ($(jjSearchBoxHiddenSelector).val() == "") {
                    JJFeedbackIcon.setIcon(jjSearchBoxSelector, JJFeedbackIcon.warningClass);
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
                        JJFeedbackIcon.setIcon(jjSearchBoxSelector, "");
                        return frm.serializeArray();
                    },
                },
                onSelect: function (item) {
                    $(jjSearchBoxHiddenSelector).val(item.value);
                    if (item.value != "") {
                        JJFeedbackIcon.setIcon(jjSearchBoxSelector, JJFeedbackIcon.successClass);
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
class JJSlider {
    static observeSliders() {
        let sliders = document.getElementsByClassName("jjslider");
        Array.from(sliders).forEach((slider) => {
            let sliderInput = document.getElementById(slider.id + "-value");
            document.getElementById(slider.id).addEventListener('change', function () {
                this.setAttribute('value', (this).value);
            });
            slider.oninput = function () {
                sliderInput.value = (this).value;
            };
        });
    }
    static observeInputs() {
        let inputs = document.getElementsByClassName("jjslider-value");
        Array.from(inputs).forEach((input) => {
            let slider = document.getElementById(input.id.replace("-value", ""));
            input.oninput = function () {
                slider.value = (this).value;
            };
        });
    }
}
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
class JJTabNav {
    static setup() {
        $("a.jj-tab-link").on("shown.bs.tab", function (e) {
            var link = $(e.target);
            $("#" + link.attr("jj-objectid")).val(link.attr("jj-tabindex"));
        });
    }
}
class JJTextArea {
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
class JJUpload {
    static setup() {
        $("div.fileUpload").each(function () {
            let obj = $(this);
            let objid = obj.attr("id");
            let multiple = (obj.attr("jjmultiple") === "true");
            let autoSubmit = (obj.attr("autoSubmit") === "true");
            let maxFileSize = obj.attr("maxFileSize");
            let dragDrop = obj.attr("dragDrop");
            let copyPaste = obj.attr("copyPaste");
            let showFileSize = obj.attr("showFileSize");
            let allowedTypes = obj.attr("allowedTypes");
            let dragDropStr = "<span>&nbsp;<b>" + obj.attr("dragDropStr") + "</b></span>";
            let acceptFiles = "*";
            if (allowedTypes != "*") {
                acceptFiles = "." + allowedTypes.replace(" ", "").replace(",", ",.");
            }
            let frm = $("form");
            let surl = frm.attr("action");
            if (surl.includes("?"))
                surl += "&t=jjupload";
            else
                surl += "?t=jjupload";
            surl += "&objname=" + objid;
            let upload = $("#" + objid).uploadFile({
                url: surl,
                formData: frm.serializeArray(),
                fileName: "file",
                multiple: multiple,
                maxFileSize: maxFileSize,
                maxFileCount: 1000,
                dragDrop: dragDrop,
                showFileSize: showFileSize,
                dragdropWidth: ($(this).width() - 10),
                statusBarWidth: ($(this).width() - 10),
                autoSubmit: true,
                uploadButtonClass: bootstrapVersion == 3 ? "btn btn-default" : "btn btn-outline-dark",
                allowedTypes: allowedTypes,
                acceptFiles: acceptFiles,
                uploadStr: obj.attr("uploadStr"),
                dragDropStr: dragDropStr,
                doneStr: obj.attr("doneStr"),
                cancelStr: obj.attr("cancelStr"),
                abortStr: obj.attr("abortStr"),
                extErrorStr: obj.attr("extErrorStr"),
                sizeErrorStr: obj.attr("sizeErrorStr"),
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
                afterUploadAll: function (obj) {
                    if (autoSubmit && obj.selectedFiles > 0) {
                        $("#uploadaction_" + objid).val("afteruploadall");
                        $("form:first").submit();
                    }
                }
            });
            $(window).resize(function () {
                $("#" + objid + " .ajax-upload-dragdrop").width($("#" + objid).width() - 30 + "px");
            });
            if (copyPaste == "true") {
                window.addEventListener('paste', e => {
                    var files = e.clipboardData.files;
                    if (files.length == 1) {
                        var file = files[0];
                        if (file.type.indexOf("image") != -1) {
                            $("#btnDoUpload_" + objid).click(function () {
                                $("#preview_modal_" + objid).modal("hide");
                                var filename = $("#preview_filename_" + objid).val();
                                if (filename.trim() == "") {
                                    filename = "image";
                                }
                                filename += ".png";
                                const dt = new DataTransfer();
                                const myNewFile = new File([file], filename, { type: file.type });
                                dt.items.add(myNewFile);
                                $("#" + objid + " input[type='file']")[0].files = dt.files;
                                $("#" + objid + " input[type='file']").trigger("change");
                                return;
                            });
                            var reader = new FileReader();
                            reader.onload = function (event) {
                                document.getElementById("pastedimage_" + objid).src = event.target.result.toString();
                                var filename = file.name.replace(/\.[^/.]+$/, "");
                                $("#preview_filename_" + objid).val(filename);
                                $("#preview_modal_" + objid).modal();
                            };
                            reader.readAsDataURL(file);
                            return;
                        }
                    }
                    let selector = "#" + objid + " input[type='file']";
                    $()[0].files = files;
                    $("#" + objid + " input[type='file']").trigger("change");
                });
            }
        });
    }
}
var jjview = (function () {
    function tablePost(objid, enableAjax, loadform) {
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
                        $("form:first").submit();
                        return;
                    }
                    $("#jjgridview_" + objid).html(data);
                    if (loadform) {
                        jjloadform();
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
            $("form:first").submit();
        }
    }
    return {
        showExportOptions: function (objid, exportType) {
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
        },
        doClearFilter: function (objid, enableAjax) {
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
            tablePost(objid, enableAjax, false);
        },
        doFilter: function (objid, enableAjax) {
            $("#current_filteraction_" + objid).val("FILTERACTION");
            $("#current_tableaction_" + objid).val("");
            $("#current_tablepage_" + objid).val("1");
            $("#current_formaction_" + objid).val("");
            tablePost(objid, enableAjax, false);
            return false;
        },
        doSearch: function (objid, oDom) {
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
        },
        doSelectItem: function (objid, obj) {
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
                valuesList = valuesList.filter(function (item) { return item !== obj.val(); });
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
        },
        doUnSelectAll: function (objid) {
            $(".jjselect input").not(":disabled").prop("checked", false);
            $("#selectedrows_" + objid).val("");
            var oSelectedtext = $("#selectedtext_" + objid);
            oSelectedtext.text(oSelectedtext.attr("noSelStr"));
        },
        doSelectAll: function (objid) {
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
                    var aValues = data.split(",");
                    $(".jjselect input").not(":disabled").prop("checked", true);
                    $("#selectedrows_" + objid).val(aValues);
                    var oSelectedtext = $("#selectedtext_" + objid);
                    var promptStr = oSelectedtext.attr("paramSelStr").replace("{0}", aValues.length.toString());
                    oSelectedtext.text(promptStr);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.log(errorThrown);
                    console.log(textStatus);
                    console.log(jqXHR);
                }
            });
        },
        doSorting: function (objid, enableAjax, v) {
            var tableOrder = "#current_tableorder_" + objid;
            if (v + " ASC" == $(tableOrder).val())
                $(tableOrder).val(v + " DESC");
            else
                $(tableOrder).val(v + " ASC");
            $("#current_tableaction_" + objid).val("");
            $("#current_formaction_" + objid).val("");
            tablePost(objid, enableAjax, true);
        },
        doMultSorting: function (objid) {
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
            jjview.doRefresh(objid, true);
        },
        doPagination: function (objid, enableAjax, v) {
            $("#current_tablepage_" + objid).val(v);
            $("#current_tableaction_" + objid).val("");
            $("#current_formaction_" + objid).val("");
            tablePost(objid, enableAjax, true);
        },
        doRefresh: function (objid, enableAjax) {
            $("#current_tableaction_" + objid).val("");
            $("#current_tablerow_" + objid).val("");
            $("#current_formaction_" + objid).val("");
            tablePost(objid, enableAjax, true);
        },
        doConfigUI: function (objid, criptid) {
            $("#current_tableaction_" + objid).val(criptid);
            $("#current_tablepage_" + objid).val("1");
            $("#current_tablerow_" + objid).val("");
            $("#current_formaction_" + objid).val("");
            $("form:first").submit();
        },
        doConfigCancel: function (objid) {
            $("form").trigger("reset");
            $("form :checkbox").change();
            $("#config_modal_" + objid).modal("hide");
        },
        doSelElementInsert: function (objid, criptid) {
            $("#current_painelaction_" + objid).val("ELEMENTSEL");
            $("#current_selaction_" + objid).val(criptid);
            $("form:first").submit();
        },
        formAction: function (objid, criptid, confirmMessage) {
            if (confirmMessage) {
                const result = confirm(confirmMessage);
                if (!result) {
                    return false;
                }
            }
            $("#current_tableaction_" + objid).val("");
            $("#current_formaction_" + objid).val(criptid);
            $("form:first").submit();
        },
        gridAction: function (objid, criptid, confirmMessage) {
            if (confirmMessage) {
                var result = confirm(confirmMessage);
                if (!result) {
                    return false;
                }
            }
            $("#current_tableaction_" + objid).val(criptid);
            $("#current_formaction_" + objid).val("");
            $("form:first").submit();
        },
        doPainelAction: function (objid, v) {
            $("#current_painelaction_" + objid).val(v);
            $("form").submit();
            return false;
        },
        doFormUrlRedirect: function (objid, criptid, confirmMessage) {
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
        },
        doSqlCommand: function (objid, criptid, confirmMessage) {
            if (confirmMessage) {
                var result = confirm(confirmMessage);
                if (!result) {
                    return false;
                }
            }
            $("#current_tableaction_" + objid).val("");
            $("#current_formaction_" + objid).val("");
            $("#current_tablerow_" + objid).val(criptid);
            $("form:first").submit();
        },
        showInsertSucess: function (objid) {
            $("#pnl_insertmsg_" + objid).fadeOut(2000, function () {
                $("#pnl_insert_" + objid).slideDown();
            });
        },
        doUrlRedirect: function (url, ispopup, title, confirmMessage, popupSize = 1) {
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
        },
        setLookup: function (objid, value) {
            window.parent.popup.hide();
            setTimeout(function () {
                window.parent.$("#id_" + objid).val(value);
                window.parent.$("#" + objid).val(value).change().blur();
            }, 100);
        },
        deleteFile: function (objid, filename, promptStr) {
            const result = confirm(promptStr);
            if (!result) {
                return false;
            }
            $("#uploadaction_" + objid).val("DELFILE");
            $("#filename_" + objid).val(filename);
            $("form:first").submit();
        },
        downloadFile: function (objid, filename) {
            $("#uploadaction_" + objid).val("DOWNLOADFILE");
            $("#filename_" + objid).val(filename);
            $("form:first").submit();
            setTimeout(function () {
                messageWait.hide();
                $("#uploadaction_" + objid).val("");
            }, 1500);
        },
        renameFile: function (objid, filename, promptStr) {
            var newFileName = prompt(promptStr, filename);
            if (newFileName != null && newFileName != filename) {
                $("#uploadaction_" + objid).val("RENAMEFILE");
                $("#filename_" + objid).val(filename + ";" + newFileName);
                $("form:first").submit();
            }
        },
        openUploadForm: function (objid, title, values) {
            const pnlname = $("#v_" + objid).attr("pnlname");
            let surl = $("form").attr("action");
            surl += surl.includes("?") ? "&" : "?";
            surl += "jjuploadform_" + pnlname + "=" + objid;
            surl += "&uploadvalues=" + values;
            popup.show(title, surl);
        },
        directDownload: function (objid, pnlname, filename) {
            messageWait.show();
            var url = $("form").attr("action");
            url += url.includes("?") ? "&" : "?";
            url += "jjuploadform_" + pnlname + "=" + objid;
            url += "&downloadfile=" + filename;
            window.location.assign(url);
            setTimeout(function () {
                messageWait.hide();
            }, 1500);
        },
        viewLog: function (objid, id) {
            $("#viewid_" + objid).val(id);
            $("form:first").submit();
        },
        loadFrameLog: function (objId, logId) {
            $("#sortable_grid a").removeClass("active");
            if (logId != "")
                $("#" + logId).addClass("active");
            $('#viewid_' + objId).val(logId);
            const frm = $("form");
            let surl = frm.attr("action");
            if (surl.includes("?"))
                surl += "&t=ajax";
            else
                surl += "?t=ajax";
            $.post(surl, frm.serialize(), function (data) {
                $("#jjpainellog_loghistory").html(data);
            });
        }
    };
})();
var messageBox = (function () {
    const TMessageIcon = {
        NONE: 1,
        INFO: 2,
        WARNING: 3,
        ERROR: 4,
        QUESTION: 5
    };
    const TMessageSize = {
        SMALL: 1,
        DEFAULT: 2,
        LARGE: 3
    };
    var jQueryModalId = "#site-modal";
    var jQueryModalTitleId = "#site-modal-title";
    var jQueryModalContentId = "#site-modal-content";
    var jQueryModalButton1Id = "#site-modal-btn1";
    var jQueryModalButton2Id = "#site-modal-btn2";
    var modalId = jQueryModalId.substring(1);
    var button1Id = jQueryModalButton1Id.substring(1);
    function setTitle(title) {
        $(jQueryModalTitleId).html(title);
    }
    function setContent(content) {
        $(jQueryModalContentId).html(content);
    }
    function showModal() {
        if (bootstrapVersion < 5) {
            $(jQueryModalId)
                .modal()
                .on("shown.bs.modal", function () {
                $(jQueryModalButton1Id).focus();
            });
        }
        else {
            const modal = new bootstrap.Modal(document.getElementById(modalId), {});
            modal.show();
            modal.addEventListener('shown.bs.modal', function () {
                document.getElementById(button1Id).focus();
            });
        }
    }
    function setBtn1(label, func) {
        $(jQueryModalButton1Id).text(label);
        if ($.isFunction(func)) {
            $(jQueryModalButton1Id).on("click.siteModalClick1", func);
        }
        $(jQueryModalButton1Id).show();
    }
    function setBtn2(label, func) {
        $(jQueryModalButton2Id).text(label);
        if ($.isFunction(func)) {
            $(jQueryModalButton2Id).on("click.siteModalClick2", func);
        }
        $(jQueryModalButton2Id).show();
    }
    function reset() {
        setTitle("");
        setContent("");
        $(jQueryModalButton1Id).text("");
        $(jQueryModalButton1Id).off("click.siteModalClick1");
        $(jQueryModalButton2Id).text("");
        $(jQueryModalButton2Id).off("click.siteModalClick2");
    }
    function loadHtml(icontype, sizetype) {
        if ($(jQueryModalId).length) {
            $(jQueryModalId).remove();
        }
        var html = "";
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
            html += "        <button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"modal\"></button>\r\n";
        }
        else if (bootstrapVersion == 3) {
            html += "        <h4 id=\"site-modal-title\" class=\"modal-title\"><button type=\"button\" class=\"close\" data-dismiss=\"modal\">&times;</button></h4>\r\n";
        }
        html += "      </div>\r\n";
        html += "      <div class=\"modal-body\">\r\n";
        html += "        <table border=\"0\">\r\n";
        html += "          <tr>\r\n";
        html += "            <td style=\"width:40px\">\r\n";
        if (icontype == TMessageIcon.ERROR) {
            html += "              <span class=\"text-danger\">\r\n";
            html += "                <span class=\"fa fa-times-circle\" aria-hidden=\"true\" style=\"font-size: 30px;\"></span>\r\n";
            html += "              </span>\r\n";
        }
        else if (icontype == TMessageIcon.WARNING) {
            html += "              <span class=\"text-warning\">\r\n";
            html += "                <span class=\"fa fa-exclamation-triangle \" aria-hidden=\"true\" style=\"font-size: 30px;\"></span>\r\n";
            html += "              </span>\r\n";
        }
        else if (icontype == TMessageIcon.INFO) {
            html += "              <span class=\"text-info\">\r\n";
            html += "                <span class=\"fa fa-info-circle\" aria-hidden=\"true\" style=\"font-size: 30px;\"></span>\r\n";
            html += "              </span>\r\n";
        }
        else if (icontype == TMessageIcon.QUESTION) {
            html += "              <span class=\"text-info\">\r\n";
            html += "                <span class=\"fa fa-question-circle\" aria-hidden=\"true\" style=\"font-size: 30px;\"></span>\r\n";
            html += "              </span>\r\n";
        }
        html += "            </td>\r\n";
        html += "            <td>\r\n";
        html += "              <span id=\"site-modal-content\"></span>\r\n";
        html += "            </td>\r\n";
        html += "          </tr>\r\n";
        html += "        </table>\r\n";
        html += "      </div>\r\n";
        html += "      <div class=\"modal-footer\">\r\n";
        if (bootstrapVersion == 3) {
            html += "        <button type=\"button\" id=\"site-modal-btn1\" class=\"btn btn-default\" data-dismiss=\"modal\"></button>\r\n";
            html += "        <button type=\"button\" id=\"site-modal-btn2\" class=\"btn btn-default\" data-dismiss=\"modal\"></button>\r\n";
        }
        else if (bootstrapVersion == 4) {
            html += "        <button type=\"button\" id=\"site-modal-btn1\" class=\"btn btn-outline-dark\" data-dismiss=\"modal\"></button>\r\n";
            html += "        <button type=\"button\" id=\"site-modal-btn2\" class=\"btn btn-outline-dark\" data-dismiss=\"modal\"></button>\r\n";
        }
        else {
            html += "        <button type=\"button\" id=\"site-modal-btn1\" class=\"btn btn-outline-dark\" data-bs-dismiss=\"modal\"></button>\r\n";
            html += "        <button type=\"button\" id=\"site-modal-btn2\" class=\"btn btn-outline-dark\" data-bs-dismiss=\"modal\"></button>\r\n";
        }
        html += "      </div>\r\n";
        html += "    </div>\r\n";
        html += "  </div>\r\n";
        html += "</div>\r\n";
        $("body").append(html);
    }
    return {
        show: function (title, content, icontype, sizetype, btn1Label, btn1Func, btn2Label, btn2Func) {
            reset();
            loadHtml(icontype, sizetype);
            setTitle(title);
            setContent(content);
            if (btn1Label === undefined) {
                setBtn1("Fechar", null);
            }
            else {
                setBtn1(btn1Label, btn1Func);
            }
            if (btn2Label === undefined) {
                $(jQueryModalButton2Id).hide();
            }
            else {
                setBtn2(btn2Label, btn2Func);
            }
            showModal();
        },
        hide: function () {
            $(jQueryModalId).modal("hide");
            $(".modal-backdrop").hide();
        }
    };
})();
var messageWait = (function () {
    var htmlIdWaitModal = "#jjform_wait_message";
    function loadHtml() {
        if (!$(htmlIdWaitModal).length) {
            var html = "";
            html += "<div id=\"jjform_wait_message\">\r\n";
            html += "    <div class=\"ajaxImage\"></div>\r\n";
            html += "    <div class=\"ajaxMessage\">Aguarde...</div>\r\n";
            html += "</div>";
            $(html).insertAfter($("body"));
            var opts = {
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
            var spinner = new Spinner(opts).spin();
            $(spinner.el).insertAfter($("#jjform_wait_message .ajaxImage"));
        }
    }
    return {
        show: function () {
            loadHtml();
            $(htmlIdWaitModal).css("display", "");
        },
        hide: function () {
            $(htmlIdWaitModal).css("display", "none");
        }
    };
})();
class Popup {
    constructor() {
        this.modalId = "popup-modal";
        this.modalTitleId = "popup-modal-title";
    }
    setTitle(title) {
        document.getElementById(this.modalTitleId).innerHTML = title;
    }
    showModal() {
        if (bootstrapVersion < 5) {
            $("#" + this.modalId).modal();
        }
        else {
            const modal = new bootstrap.Modal(document.getElementById(this.modalId), {});
            modal.show();
        }
        messageWait.show();
        $("iframe").on("load", function () {
            messageWait.hide();
        });
    }
    loadHtml(url, size) {
        if ($("#" + this.modalId).length) {
            $("#" + this.modalId).remove();
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
        html += "<div id=\"popup-modal\" tabindex=\"-1\" class=\"modal fade\" role=\"dialog\">\r\n";
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
            html += "        <h4 id=\"popup-modal-title\" class=\"modal-title\"></h4>\r\n";
        }
        else {
            html += "        <h4 id=\"popup-modal-title\" class=\"modal-title\"></h4>\r\n";
            if (bootstrapVersion >= 5) {
                html += "        <button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"modal\"></button>\r\n";
            }
            else {
                html += "        <button type=\"button\" class=\"close\" data-dismiss=\"modal\">&times;</button>\r\n";
            }
        }
        html += "      </div>\r\n";
        html += "      <div class=\"modal-body\"  style=\"height:90%;width:auto;\">\r\n";
        html += "         <iframe style=\"border: 0px;\" ";
        html += " src='";
        html += url;
        html += "' width='100%' height='97%'>Waiting...</iframe>";
        html += "      </div>\r\n";
        html += "    </div>\r\n";
        html += "  </div>\r\n";
        html += "</div>\r\n";
        $(html).appendTo($("body"));
    }
    show(title, url, size = 0) {
        this.loadHtml(url, size);
        this.setTitle(title);
        this.showModal();
    }
    hide() {
        $("#" + this.modalId).modal("hide");
    }
    modal() {
        return $("#" + this.modalId);
    }
}
var popup = function () {
    if (!(this instanceof Popup)) {
        return new Popup();
    }
}();
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
var _a, _b;
var showWaitOnPost = true;
var bootstrapVersion = 3;
const locale = (_a = document.documentElement.lang) !== null && _a !== void 0 ? _a : 'pt-BR';
const localeCode = (_b = locale.split("-")[0]) !== null && _b !== void 0 ? _b : 'pt';
//# sourceMappingURL=jjmasterdata.js.map