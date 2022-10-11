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
                $("#id_" + lookupId).val(lookupInput.val());
                $("#st_" + lookupId)
                    .removeClass("fa-check")
                    .removeClass("fa-exclamation-triangle")
                    .removeClass("fa-ellipsis-h")
                    .removeClass("fa-times");

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
                            $("#st_" + lookupId)
                                .removeClass("fa fa-check")
                                .addClass("fa fa-exclamation-triangle");
                            lookupInput.removeAttr("readonly");
                        } else {
                            $("#st_" + lookupId)
                                .removeClass("fa fa-exclamation-triangle")
                                .removeClass("fa fa-ellipsis-h")
                                .addClass("fa fa-check");
                            lookupInput.attr("readonly", "readonly").val(data.description);
                        }

                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        showWaitOnPost = true;
                        lookupInput.removeClass("loading-circle");
                        $("#st_" + lookupId)
                            .removeClass("fa fa-check")
                            .addClass("fa fa-times");

                        console.log(errorThrown);
                        console.log(textStatus);
                        console.log(jqXHR);
                    }
                });

            });

        });
    }
}