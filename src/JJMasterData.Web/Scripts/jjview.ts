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
            if (exportType == "1") { //XLS
                $("#" + objid + "_div_export_orientation").hide();
                $("#" + objid + "_div_export_all").show();
                $("#" + objid + "_div_export_delimiter").hide();
                $("#" + objid + "_div_export_fistline").show();
            }
            else if (exportType == "2") { //PDF
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
                    currentObj.val("")
                }

                let inputType : string = (this as any).type;

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
                    } else if (currentObj.hasClass("jjsearchbox")) {
                        currentObj.blur();
                    } else if (currentObj.hasClass("jjlookup")) {
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
            } else {
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

            // @ts-ignore
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
                    } else {
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

        /**
         * Realiza um redirecionamento na pagina
         *
         * @param {string} url Endereço http para onde a pagina será redirecionada
         * @param {boolean} ispopup Abrir endereço como popup
         * @param {string} title [opcional] Titulo da popUp
         * @param {string} confirmMessage [opcional] Mensagem de confirmação antes de executar a ação
         */
        doUrlRedirect: function (url, ispopup, title, confirmMessage, popupSize = 1) {

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
            let url = $("form").attr("action");
            url += url.includes("?") ? "&" : "?";
            url += "jjuploadform_" + pnlname + "=" + objid;
            url += "&uploadvalues=" + values;
            popup.show(title, url , 1);
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