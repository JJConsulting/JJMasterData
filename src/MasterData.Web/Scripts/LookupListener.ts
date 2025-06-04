class LookupListener {
    static listenChanges(selectorPrefix = String()) {
        const lookupInputs = document.querySelectorAll<HTMLInputElement>(selectorPrefix + "input.jj-lookup");

        lookupInputs.forEach(lookupInput => {
            let lookupId = lookupInput.id;
            
            const urlBuilder = new UrlBuilder()
            urlBuilder.addQueryParameter("routeContext", lookupInput.getAttribute("route-context"))
            urlBuilder.addQueryParameter("fieldName", lookupInput.getAttribute("lookup-field-name"))
            const lookupDescriptionUrl = urlBuilder.build();
            
            const lookupIdSelector = "#" + lookupId;
            const lookupDescriptionSelector = lookupIdSelector + "-description";
            const lookupIdInput = document.querySelector<HTMLInputElement>(lookupIdSelector);
            const lookupDescriptionInput = document.querySelector<HTMLInputElement>(lookupDescriptionSelector);
            lookupInput.addEventListener("blur", function () {
                if(!lookupInput.value)
                    return;
                
                FeedbackIcon.removeAllIcons(lookupDescriptionSelector);
                
                postFormValues({
                    url: lookupDescriptionUrl,
                    success: (data) => {
                       
                        if (!data.description) {
                            FeedbackIcon.setIcon(lookupIdSelector, FeedbackIcon.warningClass);
                            lookupDescriptionInput.value = "";
                            lookupInput.value = "";
                        } else {
                            FeedbackIcon.setIcon(lookupIdSelector, FeedbackIcon.successClass);
                            lookupIdInput.value = data.id;
                            
                            if(lookupDescriptionInput){
                                lookupDescriptionInput.value = data.description;
                            }
                        }
                    },
                    error: (_) => {
                        FeedbackIcon.setIcon(lookupIdSelector, FeedbackIcon.errorClass);
                        if(lookupDescriptionInput){
                            lookupDescriptionInput.value = "";
                        }
                    }
                });
            });
        });
    }
}
