class JJView {
    private static postFormValues(objid, enableAjax, loadform) {
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

                    $("#jjgridview-" + objid).html(data);
                    if (loadform) {
                        loadJJMasterData();
                    }
                    $("#current-filter-action-" + objid).val("");
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.log(errorThrown);
                    console.log(textStatus);
                    console.log(jqXHR);
                    $("#current-filter-action-" + objid).val("");
                }
            });
        } else {
            $("form:first").trigger("submit");
        }
    }
    static selectItem(objid, obj) {
        var values = $("#selected-rows" + objid).val().toString();
        var valuesList = [];

        if (obj.attr("id") == "jjcheckbox-select-all-rows")
            return;

        if (values.length > 0) {
            valuesList = values.split(",");
        }

        if (obj.prop("checked")) {
            if ($.inArray(obj.val(), valuesList) < 0)
                valuesList.push(obj.val());
        } else {
            valuesList = valuesList.filter(function (item) {
                return item !== obj.val();
            });
        }

        $("#selected-rows" + objid).val(valuesList);

        var textInfo = "";
        var selectedText = $("#selected-text-" + objid);
        if (valuesList.length == 0)
            textInfo = selectedText.attr("no-record-selected-label");
        else if (valuesList.length == 1)
            textInfo = selectedText.attr("one-record-selected-label");
        else
            textInfo = selectedText.attr("multiple-records-selected-label").replace("{0}", valuesList.length.toString());

        selectedText.text(textInfo);
    }

    static unSelectAll(objid) {
        $(".jjselect input").not(":disabled").prop("checked", false);
        $("#selected-rows" + objid).val("");
        var oSelectedtext = $("#selected-text-" + objid);
        oSelectedtext.text(oSelectedtext.attr("no-record-selected-label"));
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
                GridView.selectAllRowsElements(objid, JSON.parse(data).selectedRows)
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
                console.log(textStatus);
                console.log(jqXHR);
            }
        });
    }

    static sortFormValues(objid, enableAjax, v) {
        var tableOrder = "#current-table-order-" + objid;
        if (v + " ASC" == $(tableOrder).val())
            $(tableOrder).val(v + " DESC");
        else
            $(tableOrder).val(v + " ASC");

        $("#current-table-action-" + objid).val("");
        $("#current-form-action-" + objid).val("");
        this.postFormValues(objid, enableAjax, true);
    }

    static sortItems(objid) {
        var descCommand = "";

        // @ts-ignore
        var order = $("#sortable-" + objid).sortable("toArray");

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

        $("#current-table-order-" + objid).val(descCommand);
        $("#sort-modal-" + objid).modal('hide');
        $("#current-form-action-" + objid).val("");
        this.refresh(objid, true);

    }

    static paginateGrid(objid, enableAjax, v) {
        $("#current-table-page-" + objid).val(v);
        $("#current-table-action-" + objid).val("");
        $("#current-form-action-" + objid).val("");
        this.postFormValues(objid, enableAjax, true);
    }

    static refresh(objid, enableAjax) {
        $("#current-table-action-" + objid).val("");
        $("#current-table-row-" + objid).val("");
        $("#current-form-action-" + objid).val("");
        this.postFormValues(objid, enableAjax, true);
    }

    static openSettingsModal(componentName, encryptedActionMap) {
        $("#current-table-action-" + componentName).val(encryptedActionMap);
        $("#current-table-page-" + componentName).val("1");
        $("#current-table-row-" + componentName).val("");
        $("#current-form-action-" + componentName).val("");
        $("form:first").trigger("submit");
    }
    static closeSettingsModal(objid) {
        $("form").trigger("reset");
        $("form :checkbox").change();
        $("#config-modal-" + objid).modal("hide");
    }


    static filter(objid, enableAjax) {
        $("#current-filter-action-" + objid).val("FILTERACTION");
        $("#current-table-action-" + objid).val("");
        $("#current-table-page-" + objid).val("1");
        $("#current-form-action-" + objid).val("");
        this.postFormValues(objid, enableAjax, false);
        return false;
    }

    static openSelectElementInsert(componentName, encryptedActionMap) {
        $("#current-panel-action-" + componentName).val("ELEMENTSEL");
        $("#current-select-action-values" + componentName).val(encryptedActionMap);
        $("form:first").trigger("submit");
    }

    static clearFilter(objid, enableAjax) {
        var divId = "#current-grid-filter-" + objid;
        var selector = divId + " input:enabled, " + divId + " select:enabled";

        $(selector).each(function () {
            let currentObj = $(this);

            if (currentObj.hasClass("flatpickr-input")) {
                currentObj.val("")
            }

            let inputType: string = (this as any).type;

            if (inputType == "checkbox") {
                currentObj.prop("checked", false);
            } else if (inputType != "input" && currentObj.attr("data-role") == "tagsinput") {
                currentObj.tagsinput('removeAll');
            } else if (inputType != "hidden") {
                currentObj.val("");
                if (currentObj.hasClass("selectpicker")) {
                    currentObj.selectpicker("render");
                } else if (currentObj.hasClass("jjsearchbox")) {
                    currentObj.blur();
                } else if (currentObj.hasClass("jjlookup")) {
                    currentObj.blur();
                }
            }
        });
        $("#current-filter-action-" + objid).val("CLEARACTION");
        $("#current-table-action-" + objid).val("");
        $("#current-form-action-" + objid).val("");
        this.postFormValues(objid, enableAjax, false);
    }

    static executeGridAction(componentName, encryptedActionMap, confirmMessage) {
        if (confirmMessage) {
            var result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }

        $("#current-table-action-" + componentName).val(encryptedActionMap);
        $("#current-form-action-" + componentName).val("");
        $("form:first").trigger("submit");
    }
    

    static executeSqlCommand(objid, criptid, confirmMessage) {
        if (confirmMessage) {
            var result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }

        $("#current-table-action-" + objid).val("");
        $("#current-form-action-" + objid).val("");
        $("#current-table-row-" + objid).val(criptid);
        $("form:first").trigger("submit");
    }

    static setLookup(objid, value) {
        window.parent.popup.hide();
        setTimeout(function () {
            window.parent.$("#id_" + objid).val(value);
            window.parent.$("#" + objid).val(value).change().blur();
        }, 100);

    }

    /**
     * Realiza um redirecionamento na pagina
     *
     * @param {string} url Endereço http para onde a pagina será redirecionada
     * @param {boolean} ispopup Abrir endereço como popup
     * @param {string} title [opcional] Titulo da popUp
     * @param {string} confirmMessage [opcional] Mensagem de confirmação antes de executar a ação
     */
    private static executeUrlRedirect(url, ispopup, title, confirmMessage, popupSize = 1) {

        if (confirmMessage) {
            const result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }

        if (ispopup) {
            popup.show(title, url, popupSize);
        } else {
            window.location.href = url;
        }
    }

    static showInsertSucess(objid) {
        $("#insert-message-panel" + objid).fadeOut(2000, function () {
            $("#insert-panel" + objid).slideDown();
        });
    }

    static deleteFile(objid, filename, promptStr) {
        const result = confirm(promptStr);
        if (!result) {
            return false;
        }
        $("#upload-action-" + objid).val("DELFILE");
        $("#filename-" + objid).val(filename);
        $("form:first").trigger("submit");
    }

    static downloadFile(objid, filename) {
        $("#upload-action-" + objid).val("DOWNLOADFILE");
        $("#filename-" + objid).val(filename);
        $("form:first").trigger("submit");
        setTimeout(function () {
            SpinnerOverlay.hide();
            $("#upload-action-" + objid).val("");
        }, 1500);
    }

    static renameFile(objid, filename, promptStr) {
        var newFileName = prompt(promptStr, filename);
        if (newFileName != null && newFileName != filename) {
            $("#upload-action-" + objid).val("RENAMEFILE");
            $("#filename-" + objid).val(filename + ";" + newFileName);
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
        if (exportType == "1") { //XLS
            $("#" + objid + "-div-export-orientation").hide();
            $("#" + objid + "-div-export-all").show();
            $("#" + objid + "-div-export-delimiter").hide();
            $("#" + objid + "-div-export-firstline").show();
        } else if (exportType == "2") { //PDF
            $("#" + objid + "-div-export-orientation").show();
            $("#" + objid + "-div-export-all").hide();
            $("#" + objid + "-div-export-delimiter").hide();
            $("#" + objid + "-div-export-firstline").hide();
        } else {
            $("#" + objid + "-div-export-orientation").hide();
            $("#" + objid + "-div-export-all").show();
            $("#" + objid + "-div-export-delimiter").show();
            $("#" + objid + "-div-export-firstline").show();
        }
    }

    static viewLog(objid, id) {

        $("#logId-" + objid).val(id);
        $("form:first").trigger("submit");
    }

    static searchOnDOM(objid, oDom) {
        var value = $(oDom).val().toString().toLowerCase();
        $("#table_" + objid + " tr").filter(<any>function () {
            //procura por textos
            var textValues = $(this).clone().find('.bootstrap-select, .selectpicker, select').remove().end().text();
            var isSearch = textValues.toLowerCase().indexOf(value) > -1;

            //se não achou procura nos inputs
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

            //se não achou procura nas combos
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
        } else {
            $("#infotext_" + objid).css("display", "");
            $("ul.pagination").css("display", "");
        }
    }
}
