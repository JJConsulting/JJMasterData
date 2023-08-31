class LookupListener {
    static listenChanges() {
        const lookupInputs = document.querySelectorAll<HTMLInputElement>("input.jj-lookup");

        lookupInputs.forEach(lookupInput => {
            let lookupId = lookupInput.id;
            let fieldName = lookupInput.getAttribute("lookup-field-name");
            let panelName = lookupInput.getAttribute("panelName");
            let lookupDescriptionUrl = lookupInput.getAttribute("lookup-description-url");
            
            const lookupIdSelector = "#" + lookupId;
            const lookupDescriptionSelector = lookupIdSelector + "-description";
            
            lookupInput.addEventListener("blur", function () {
                showWaitOnPost = false;

                FeedbackIcon.removeAllIcons(lookupDescriptionSelector);

                lookupInput.removeAttribute("readonly");
                if (lookupInput.value === "") {
                    return;
                }

                lookupInput.classList.add("loading-circle");

                postFormValues({
                    url: lookupDescriptionUrl,
                    success: (data) => {
                        showWaitOnPost = true;
                        lookupInput.classList.remove("loading-circle");
                        if (data.description === "") {
                            FeedbackIcon.setIcon(lookupIdSelector, FeedbackIcon.warningClass);
                        } else {
                            const lookupIdInput = document.querySelector<HTMLInputElement>(lookupIdSelector);
                            const lookupDescriptionInput = document.querySelector<HTMLInputElement>(lookupDescriptionSelector);
                            FeedbackIcon.setIcon(lookupDescriptionSelector, FeedbackIcon.successClass);
                            lookupIdInput.value = data.description;
                            lookupDescriptionInput.value = data.id;
                        }
                    },
                    error: (_) => {
                        showWaitOnPost = true;
                        lookupInput.classList.remove("loading-circle");
                        FeedbackIcon.setIcon(lookupDescriptionSelector, FeedbackIcon.errorClass);
                    }
                });
            });
        });
    }
}
