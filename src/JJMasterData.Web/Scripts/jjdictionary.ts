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
            // @ts-ignore
            $("#sortable_" + context).sortable({
                update: function () {
                    // @ts-ignore
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
                                //Session expired
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
                        //Session expired
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
                popup.hide()
            else
                // @ts-ignore
                window.parent.popup.hide()

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