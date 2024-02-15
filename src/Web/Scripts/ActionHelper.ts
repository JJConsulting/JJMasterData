class ActionHelper {
    static submitWithScrollPosition(){
        localStorage.setItem('masterDataScrollPosition', window.scrollY.toString());
        document.forms[0].submit();
    }
    
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
        const formViewActionInput = document.querySelector<HTMLInputElement>("#current-action-map-" + componentName);
        
        if(gridViewActionInput){
            gridViewActionInput.value = encryptedActionMap;
        }
        else if(formViewActionInput){
            formViewActionInput.value = encryptedActionMap;
        }
        
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", encryptedRouteContext);
        
        postFormValues({url:urlBuilder.build(), success: data => {
            TooltipHelper.dispose("#" + componentName)
            HTMLHelper.setOuterHTML(componentName,data);
            listenAllEvents("#" + componentName);
        }})
    }

    static executeRedirectAction(componentName: string, routeContext: string, encryptedActionMap: string, confirmationMessage?: string) {
        if (confirmationMessage) {
            const result = confirm(confirmationMessage);
            if (!result) {
                return false;
            }
        }

        const gridViewActionInput = document.querySelector<HTMLInputElement>("#grid-view-action-map-" + componentName);
        const formViewActionInput = document.querySelector<HTMLInputElement>("#current-action-map-" + componentName);

        if (formViewActionInput) {
            formViewActionInput.value = encryptedActionMap;
        } else {
            const newFormInput = document.createElement("input");
            newFormInput.id = "current-action-map-" + componentName;
            newFormInput.name = "current-action-map-" + componentName;
            newFormInput.type = "hidden";
            newFormInput.value = encryptedActionMap;
            document.querySelector('form').appendChild(newFormInput);
        }
        
        if (gridViewActionInput) {
            gridViewActionInput.value = encryptedActionMap;
        }
        else{
            const newGridInput = document.createElement("input");
            newGridInput.id = "grid-view-action-map-" + componentName;
            newGridInput.name = "grid-view-action-map-" + componentName;
            newGridInput.type = "hidden";
            newGridInput.value = encryptedActionMap;
            document.querySelector('form').appendChild(newGridInput);
        }
   
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        urlBuilder.addQueryParameter("componentName", componentName);

        const url = urlBuilder.build();

        this.executeUrlRedirect(url);

        return true;
    }

    static doUrlRedirect(url, isModal, modalTitle,modalSize, isIframe,confirmationMessage) {
        if (confirmationMessage) {
            const result = confirm(confirmationMessage);
            if (!result) {
                return false;
            }
        }
        if (isModal) {
            if(isIframe){
                defaultModal.showIframe(url, modalTitle, modalSize);
            }
            else{
                defaultModal.showUrl(url, modalTitle, modalSize);
            }
        }
        else {
            window.location.href = url;
        }
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
            isModal,
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
        const formViewActionInput = document.querySelector<HTMLInputElement>("#current-action-map-" + componentName);

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

        if (isModal) {
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("routeContext", formViewRouteContext);

            const modal = new Modal();
            modal.modalId = componentName + "-modal";
            modal.onModalHidden = function(){
                formViewActionInput.value = "";
            }
            SpinnerOverlay.show();
            const requestOptions = getRequestOptions();
            modal.showUrl({
                url: urlBuilder.build(), requestOptions: requestOptions
            }, modalTitle).then(function (data) {
                SpinnerOverlay.hide();
                listenAllEvents("#" + modal.modalId + " ")    
                
                if (typeof data === "object") {
                    if (data.closeModal) {
                        if(isSubmit){
                            ActionHelper.submitWithScrollPosition();
                        }
                        else{
                            GridViewHelper.refresh(componentName,gridViewRouteContext)
                            modal.remove();
                        }
                    }
                }
            })
        } else {
            if (!isSubmit) {
                const urlBuilder = new UrlBuilder();
                urlBuilder.addQueryParameter("routeContext", formViewRouteContext);

                postFormValues({url:urlBuilder.build(), success:(data)=>{
                        if (typeof data === "string") {
                            TooltipHelper.dispose("#" + componentName)
                            HTMLHelper.setOuterHTML(componentName,data);
                            listenAllEvents("#" + componentName);
                        }
                        else{
                            if(data.jsCallback){
                                eval(data.jsCallback)
                            }
                        }
                }});
            } 
            else {
                ActionHelper.submitWithScrollPosition();
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