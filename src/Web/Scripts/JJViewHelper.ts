
//TODO: Separar em GridView e FormView
class JJViewHelper {
    private static postFormValues(componentName, enableAjax, loadform) {
        if (enableAjax) {
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("context", "htmlContent");
            urlBuilder.addQueryParameter("componentName", componentName);

            postFormValues({
                url: urlBuilder.build(),
                success: function (data) {
                    const gridViewElement = document.querySelector<HTMLInputElement>("#grid-view-" + componentName);
                    const filterActionElement = document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName);

                    if (gridViewElement && filterActionElement) {
                        gridViewElement.innerHTML = data;
                        if (loadform) {
                            loadJJMasterData();
                        }
                        filterActionElement.value = "";
                    } else {
                        console.error("One or both of the elements were not found.");
                    }
                },
                error: function (error) {
                    console.error(error);
                    const filterActionElement = document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName);

                    if (filterActionElement) {
                        filterActionElement.value = "";
                    } else {
                        console.error("Filter action element was not found.");
                    }
                }
            });

        } else {
            $("form:first").trigger("submit");
        }
    }
    static selectItem(objid, obj) {
        var values = $("#grid-view-selected-rows" + objid).val().toString();
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

        $("#grid-view-selected-rows" + objid).val(valuesList);

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
        $("#grid-view-selected-rows" + objid).val("");
        var oSelectedtext = $("#selected-text-" + objid);
        oSelectedtext.text(oSelectedtext.attr("no-record-selected-label"));
    }

    static selectAll(componentName) {
        const urlBuilder = new UrlBuilder()
        urlBuilder.addQueryParameter("context","selectAll")
        
        postFormValues({
            url: urlBuilder.build(),
            success: (data)=>{
                GridViewHelper.selectAllRowsElements(componentName, data.selectedRows)
            }
        })
    }

    static sortFormValues(objid, enableAjax, v) {
        var tableOrder = "#grid-view-order-" + objid;
        if (v + " ASC" == $(tableOrder).val())
            $(tableOrder).val(v + " DESC");
        else
            $(tableOrder).val(v + " ASC");

        $("#grid-view-action-" + objid).val("");
        $("#form-view-action-map-" + objid).val("");
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

        $("#grid-view-order-" + objid).val(descCommand);
        $("#sort-modal-" + objid).modal('hide');
        $("#form-view-action-map-" + objid).val("");
        this.refresh(objid, true);

    }

    static paginateGrid(objid, enableAjax, v) {
        $("#grid-view-page-" + objid).val(v);
        $("#grid-view-action-" + objid).val("");
        $("#form-view-action-map-" + objid).val("");
        this.postFormValues(objid, enableAjax, true);
    }

    static refresh(componentName, enableAjax) {
        $("#grid-view-action-" + componentName).val("");
        $("#grid-view-row-" + componentName).val("");
        $("#form-view-action-map-" + componentName).val("");
        this.postFormValues(componentName, enableAjax, true);
    }

    static openSettingsModal(componentName, encryptedActionMap) {
        $("#grid-view-action-" + componentName).val(encryptedActionMap);
        $("#grid-view-page-" + componentName).val("1");
        $("#grid-view-row-" + componentName).val("");
        $("#form-view-action-map-" + componentName).val("");
        $("form:first").trigger("submit");
    }
    static closeSettingsModal(objid) {
        $("form").trigger("reset");
        $("form :checkbox").change();
        $("#config-modal-" + objid).modal("hide");
    }


    static filter(objid, enableAjax) {
        $("#grid-view-filter-action-" + objid).val("FILTERACTION");
        $("#grid-view-action-" + objid).val("");
        $("#grid-view-page-" + objid).val("1");
        $("#form-view-action-map-" + objid).val("");
        this.postFormValues(objid, enableAjax, false);
        return false;
    }

    static openSelectElementInsert(componentName, encryptedActionMap) {
        $("#form-view-current-action-" + componentName).val("ELEMENTSEL");
        $("#form-view-select-action-values" + componentName).val(encryptedActionMap);
        $("form:first").trigger("submit");
    }

    static clearFilter(componentName, enableAjax) {
        GridViewHelper.clearFilterInputs(componentName)
        this.postFormValues(componentName, enableAjax, false);
    }

    static executeGridAction(componentName, encryptedActionMap, confirmMessage) {
        if (confirmMessage) {
            var result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }

        $("#grid-view-action-" + componentName).val(encryptedActionMap);
        $("#form-view-action-map-" + componentName).val("");
        $("form:first").trigger("submit");
    }
    

    static executeSqlCommand(objid, criptid, confirmMessage) {
        if (confirmMessage) {
            var result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }

        $("#grid-view-action-" + objid).val("");
        $("#form-view-action-map-" + objid).val("");
        $("#grid-view-row-" + objid).val(criptid);
        $("form:first").trigger("submit");
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
            defaultModal.showIframe(url,title,  popupSize);
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

    static directDownload(objid, panelName, filename) {
        SpinnerOverlay.show();
        var url = $("form").attr("action");
        url += url.includes("?") ? "&" : "?";
        url += "uploadView-" + panelName + "=" + objid;
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

        $("#audit-log-id-" + objid).val(id);
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
