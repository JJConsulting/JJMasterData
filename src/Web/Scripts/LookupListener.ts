class LookupListener {
    static listenChanges() {
        $("input.jjlookup").each(function () {
            let lookupInput = $(this);
            let lookupId = lookupInput.attr("id");
            let fieldName = lookupInput.attr("lookup-field-name");
            let panelName = lookupInput.attr("panelName");
            let popupTitle = lookupInput.attr("popuptitle");
            let lookupUrl = lookupInput.attr("lookup-url");
            let lookupResultUrl = lookupInput.attr("lookup-result-url");
            let popupSize: number = +lookupInput.attr("popupsize");


            const jjLookupSelector = "#" + lookupId + "";
            const jjHiddenLookupSelector = "#id_" + lookupId + "";

            $("#btn_" + lookupId).on("click", function () {
                defaultModal.showIframe(lookupUrl, popupTitle, popupSize);
            });

            function setHiddenLookup() {
                $("#" + lookupId).val(lookupInput.val())
            }

          

            lookupInput.one("change", function () {
                $("#id_" + lookupId).val(lookupInput.val());
            });

            lookupInput.one("blur", function () {
                showWaitOnPost = false;
                setHiddenLookup();

                FeedbackIcon.removeAllIcons(jjLookupSelector)

                lookupInput.removeAttr("readonly");
                if (lookupInput.val() == "") {
                    return;
                }


                lookupInput.addClass("loading-circle");

                postFormValues({
                    url: lookupResultUrl, success: (data) => {
                        showWaitOnPost = true;
                        lookupInput.removeClass("loading-circle");
                        if (data.description === "") {
                            FeedbackIcon.setIcon(jjLookupSelector, FeedbackIcon.warningClass);
                        } else {
                            const lookupHiddenInputElement = document.querySelector<HTMLInputElement>("#id_" + lookupId);
                            const lookupInputElement = document.querySelector<HTMLInputElement>("#" + lookupId);
                            FeedbackIcon.setIcon(jjLookupSelector, FeedbackIcon.successClass);
                            lookupInputElement.value = data.description;
                            lookupHiddenInputElement.value = data.id;

                        
                        }
                    }, error: (_) => {
                        showWaitOnPost = true;
                        lookupInput.removeClass("loading-circle");
                        FeedbackIcon.setIcon(jjLookupSelector, FeedbackIcon.errorClass);
                    }
                })

            });
        });
    }
}