class LookupListener {
    static listenChanges() {
        const lookupInputs = document.querySelectorAll<HTMLInputElement>("input.jjlookup");

        lookupInputs.forEach(lookupInput => {
            let lookupId = lookupInput.id;
            let fieldName = lookupInput.getAttribute("lookup-field-name");
            let panelName = lookupInput.getAttribute("panelName");
            let popupTitle = lookupInput.getAttribute("popuptitle");
            let lookupUrl = lookupInput.getAttribute("lookup-url");
            let lookupResultUrl = lookupInput.getAttribute("lookup-result-url");
            let popupSize = +lookupInput.getAttribute("popupsize");

            const lookupSelector = "#" + lookupId;
            const hiddenLookupSelector = "#id_" + lookupId;

            document.querySelector("#btn_" + lookupId)?.addEventListener("click", async () => {
                await defaultModal.showUrl({ url: lookupUrl }, popupTitle, popupSize);
            });

            function setHiddenLookup() {
                document.querySelector<HTMLInputElement>(lookupSelector).value = lookupInput.value;
            }

            lookupInput.addEventListener("change", function () {
                document.querySelector<HTMLInputElement>(hiddenLookupSelector).value = lookupInput.value;
            });

            lookupInput.addEventListener("blur", function () {
                showWaitOnPost = false;
                setHiddenLookup();

                FeedbackIcon.removeAllIcons(lookupSelector);

                lookupInput.removeAttribute("readonly");
                if (lookupInput.value === "") {
                    return;
                }

                lookupInput.classList.add("loading-circle");

                postFormValues({
                    url: lookupResultUrl,
                    success: (data) => {
                        showWaitOnPost = true;
                        lookupInput.classList.remove("loading-circle");
                        if (data.description === "") {
                            FeedbackIcon.setIcon(lookupSelector, FeedbackIcon.warningClass);
                        } else {
                            const lookupHiddenInputElement = document.querySelector<HTMLInputElement>(hiddenLookupSelector);
                            const lookupInputElement = document.querySelector<HTMLInputElement>(lookupSelector);
                            FeedbackIcon.setIcon(lookupSelector, FeedbackIcon.successClass);
                            lookupInputElement.value = data.description;
                            lookupHiddenInputElement.value = data.id;
                        }
                    },
                    error: (_) => {
                        showWaitOnPost = true;
                        lookupInput.classList.remove("loading-circle");
                        FeedbackIcon.setIcon(lookupSelector, FeedbackIcon.errorClass);
                    }
                });
            });
        });
    }
}
