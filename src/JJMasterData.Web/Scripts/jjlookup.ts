class JJLookup{
    static setup(){
        $("input.jjlookup").each(function () {
            let lookupInput = $(this);
            let lookupId = lookupInput.attr("id");
            let panelName = lookupInput.attr("pnlname");
            let popupTitle = lookupInput.attr("popuptitle");
            let popupSize : number = +lookupInput.attr("popupsize");
            let form = $("form");
            let url : string = form.attr("action");

            if (url.includes("?"))
                url += "&";
            else
                url += "?";

            url += "jjlookup_";
            url += panelName + "=" + lookupId;

            const jjLookupSelector = "#" + lookupId + "";
            const jjHiddenLookupSelector = "#id_" + lookupId + "";
            
            $("#btn_" + lookupId).on("click",function () {

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

            lookupInput.on("focus",function () {
                lookupInput.val($("#id_" + lookupId).val())
                    .removeAttr("readonly")
                    .select();
            });

            lookupInput.on("change",function () {
                $("#id_" + lookupId).val(lookupInput.val());
            });

            lookupInput.on("blur",function () {
                showWaitOnPost = false;
                $(jjHiddenLookupSelector).val(lookupInput.val());
                JJFeedbackIcon.removeAllIcons(jjLookupSelector)

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
                            JJFeedbackIcon.setIcon(jjLookupSelector, JJFeedbackIcon.warningClass)
                            lookupInput.removeAttr("readonly");
                        } else {
                            JJFeedbackIcon.setIcon(jjLookupSelector, JJFeedbackIcon.successClass)
                            lookupInput.attr("readonly", "readonly").val(data.description);
                        }

                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        showWaitOnPost = true;
                        lookupInput.removeClass("loading-circle");
                        JJFeedbackIcon.setIcon(jjLookupSelector, JJFeedbackIcon.errorClass)

                        console.log(errorThrown);
                        console.log(textStatus);
                        console.log(jqXHR);
                    }
                });

            });

        });
    }
}