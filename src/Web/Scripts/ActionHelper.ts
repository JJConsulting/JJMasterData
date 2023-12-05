class ActionHelper {

    static executeSqlCommand(
        componentName: string,
        encryptedActionMap: string, 
        encryptedRouteContext: string,
        confirmMessage: string) {
        
        if (confirmMessage) {
            const result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }

        const gridViewActionInput = document.querySelector<HTMLInputElement>("#grid-view-action-map-" + componentName);
        const formViewActionInput = document.querySelector<HTMLInputElement>("#form-view-action-map-" + componentName);
        
        if(gridViewActionInput){
            gridViewActionInput.value = encryptedActionMap;
        }
        else if(formViewActionInput){
            formViewActionInput.value = encryptedActionMap;
        }
        
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", encryptedRouteContext);
        
        postFormValues({url:urlBuilder.build(), success: data => {
            document.getElementById(componentName).innerHTML = data;
        }})
    }

    static executeRedirectAction(componentName: string, routeContext: string, encryptedActionMap: string, confirmationMessage?: string) {
        if (confirmationMessage) {
            const result = confirm(confirmationMessage);
            if (!result) {
                return false;
            }
        }

        const currentFormActionInput = document.querySelector<HTMLInputElement>("#form-view-action-map-" + componentName);
        if(currentFormActionInput){
            currentFormActionInput.value = encryptedActionMap;
        }
        else{
            const newInput = document.createElement("input");
            newInput.id = "form-view-action-map-" + componentName;
            newInput.name = "form-view-action-map-" + componentName;
            newInput.value = encryptedActionMap;
            document.querySelector('form').appendChild(newInput);
        }

        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        urlBuilder.addQueryParameter("componentName", componentName);

        const url = urlBuilder.build();

        this.executeUrlRedirect(url);

        return true;
    }

    private static executeUrlRedirect(url: string) {
        postFormValues({
            url: url,
            success: (data: UrlRedirectModel) => {
                if (data.urlAsModal) {
                    if(data.isIframe){
                        defaultModal.showIframe(data.urlRedirect, data.modalTitle, data.modalSize);
                    } 
                    else{
                        defaultModal.showUrl(data.urlRedirect, data.modalTitle, data.modalSize);
                    }
                } else {
                    window.location.href = data.urlRedirect;
                }
            }
        })
    }

    private static executeInternalRedirect(url: string, modalSize: ModalSize, confirmationMessage: string) {
        if (confirmationMessage) {
            if (!confirm(confirmationMessage)) {
                return false;
            }
        }

        defaultModal.showIframe(url, "", modalSize);
    }
    

    static executeActionData(actionData: ActionData){
        const {
            componentName,
            actionMap,
            modalTitle,
            modalRouteContext,
            gridViewRouteContext,
            formViewRouteContext,
            isSubmit,
            confirmationMessage
        } = actionData;
        
        if (confirmationMessage) {
            if (!confirm(confirmationMessage)) {
                return false;
            }
        }

        const gridViewActionInput = document.querySelector<HTMLInputElement>("#grid-view-action-map-" + componentName);
        const formViewActionInput = document.querySelector<HTMLInputElement>("#form-view-action-map-" + componentName);

        if (gridViewActionInput) {
            gridViewActionInput.value = "";
        }
        if (formViewActionInput) {
            formViewActionInput.value = actionMap;
        }

        let form = document.querySelector<HTMLFormElement>("form");

        if (!form) {
            return;
        }

        if (modalRouteContext) {
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("routeContext", modalRouteContext);

            const modal = new Modal();
            modal.modalId = componentName + "-modal";
            
            SpinnerOverlay.show();
            
            modal.showUrl({
                url: urlBuilder.build(), requestOptions: {
                    method: "POST",
                    body: new FormData(document.querySelector("form"))
                }
            }, modalTitle).then(function (data) {

                SpinnerOverlay.hide();

                listenAllEvents("#" + modal.modalId + " ")    
                
                if (typeof data === "object") {
                    if (data.closeModal) {
                        GridViewHelper.refresh(componentName,gridViewRouteContext)
                        modal.remove();
                    }
                }
            })
        } else {
            if(!isSubmit){
                const urlBuilder = new UrlBuilder();
                urlBuilder.addQueryParameter("routeContext", formViewRouteContext);

                postFormValues({url:urlBuilder.build(), success:(data)=>{
                        if (typeof data === "string") {
                            HTMLHelper.setInnerHTML(componentName,data);
                            listenAllEvents("#" + componentName)
                        }
                        else{
                            if(data.jsCallback){
                                eval(data.jsCallback)
                            }
                        }
                }});
            } 
            else{
                document.forms[0].requestSubmit();
            }
        }
    }
    
    static executeAction(actionDataJson: string){
        const actionData = JSON.parse(actionDataJson);
        
        return this.executeActionData(actionData);
    }
    
    static hideActionModal(componentName:string){
        const modal = new Modal();
        modal.modalId = componentName + "-modal";
        modal.remove();
    }
    
    static launchUrl(url, isModal, title, confirmationMessage, modalSize = 1) {
        if (confirmationMessage) {
            const result = confirm(confirmationMessage);
            if (!result) {
                return false;
            }
        }

        if (isModal) {
            popup.show(title, url, modalSize);
        } else {
            window.location.href = url;
        }
    }
}