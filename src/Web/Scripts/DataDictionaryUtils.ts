class DataDictionaryUtils {
    static deleteAction(actionName: string, url: string, questionStr: string): void {
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
                    } else {
                        console.log(xhr);
                    }
                }
            });
        }
    }

    static sortAction(context: string, url: string, errorStr: string): void {
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
                            } else {
                                messageBox.show("JJMasterData", err.message, 4);
                            }
                        } else {
                            messageBox.show("JJMasterData", errorStr, 4);
                        }
                    }
                });
            }
        }).disableSelection();
    }

    static setDisableAction(isDisable: boolean, url: string, errorStr: string): void {
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
                    } else {
                        messageBox.show("JJMasterData", err.message, 4);
                    }
                } else {
                    messageBox.show("JJMasterData", errorStr, 4);
                }
            }
        });
    }

    static refreshAction(isPopup = false): void {
        SpinnerOverlay.show();

        if (isPopup) {
            window.parent.popup.hide();
            window.parent.document.forms[0].submit();
        } else {
            popup.hide();
            document.forms[0].submit();
        }
    }

    static postAction(url: string): void {
        SpinnerOverlay.show();
        $("form:first").attr("action", url).submit();
    }

    static exportElement(id: string, url: string, validStr: string): boolean {
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
