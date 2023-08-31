class LookupListener {
    static listenChanges() {
        const lookupInputs = document.querySelectorAll<HTMLInputElement>("input.jj-lookup");

        lookupInputs.forEach(lookupInput => {
            let lookupId = lookupInput.id;
            let lookupDescriptionUrl = lookupInput.getAttribute("lookup-description-url");
            
            const lookupIdSelector = "#" + lookupId;
            const lookupDescriptionSelector = lookupIdSelector + "-description";
            
            lookupInput.addEventListener("blur", function () {
                FeedbackIcon.removeAllIcons(lookupDescriptionSelector);
                
                postFormValues({
                    url: lookupDescriptionUrl,
                    success: (data) => {
                        if (data.description === "") {
                            FeedbackIcon.setIcon(lookupIdSelector, FeedbackIcon.warningClass);
                        } else {
                            const lookupIdInput = document.querySelector<HTMLInputElement>(lookupIdSelector);
                            const lookupDescriptionInput = document.querySelector<HTMLInputElement>(lookupDescriptionSelector);
                            FeedbackIcon.setIcon(lookupDescriptionSelector, FeedbackIcon.successClass);
                            lookupIdInput.value = data.id;
                            lookupDescriptionInput.value = data.description;
                        }
                    },
                    error: (_) => {
                        FeedbackIcon.setIcon(lookupDescriptionSelector, FeedbackIcon.errorClass);
                    }
                });
            });
        });
    }
}
