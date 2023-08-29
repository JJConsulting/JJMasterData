class ActionManager {

    static executeRedirectActionAtSamePage(componentName: string, encryptedActionMap: string, confirmMessage?: string){
        this.executeRedirectAction(null,componentName,encryptedActionMap,confirmMessage);
    }
    
    static executeRedirectAction(url: string, componentName: string, encryptedActionMap: string, confirmMessage?: string){
        if (confirmMessage) {
            const result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }

        const currentFormActionInput = document.querySelector<HTMLInputElement>("#form-view-action-map-" + componentName);
        currentFormActionInput.value = encryptedActionMap;

        if(!url){
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("context", "urlRedirect");
            urlBuilder.addQueryParameter("componentName", componentName);

            url = urlBuilder.build();
        }

        this.executeUrlRedirect(url);

        return true;
    }

    private static executeUrlRedirect(url: string) {
        postFormValues({
            url: url,
            success: (data)=>{
                if (data.urlAsPopUp) {
                    popup.show(data.popUpTitle, data.urlRedirect);
                } else {
                    window.location.href = data.urlRedirect;
                }
            }
        })
    }

    static executeAction(componentName, encryptedActionMap, confirmationMessage, isModal) {
        if (confirmationMessage) {
            if (!confirm(confirmationMessage)) {
                return false;
            }
        }

        const gridViewActionInput = document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName);
        const formViewActionInput = document.querySelector<HTMLInputElement>("#form-view-action-map-" + componentName);

        if(gridViewActionInput){
            gridViewActionInput.value = null;
        }
        if(formViewActionInput){
            formViewActionInput.value = encryptedActionMap;
        }

        let form = document.querySelector<HTMLFormElement>("form");

        if (!form) {
            return;
        }

        if (isModal) {
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("context", "modal");
            
            postFormValues({
                url:urlBuilder.build(),
                success:function(data){
                    const outputElement = document.getElementById(componentName);
                    if (outputElement) {
                        if (typeof data === "object") {
                            if(data.closeModal){
                                const modal = new Modal();
                                modal.modalId = componentName +"-modal";
                                modal.modalTitleId =  componentName +"-modal-tile";

                                modal.hide();

                                JJView.refresh(componentName,true)
                            }
                        } else {
                            outputElement.innerHTML = data;
                        }
                    }
                }
            })
        } else {
            form.submit();
        }
    }

    static executeFormAction(componentName: string, encryptedActionMap: string, confirmationMessage: string) {
        this.executeAction(componentName, encryptedActionMap, confirmationMessage, false);
    }

    static executeModalAction(componentName: string, encryptedActionMap: string, confirmationMessage: string) {
        this.executeAction(componentName, encryptedActionMap, confirmationMessage, true);
    }

    static executeFormActionAsModal(componentName: string,title: string, encryptedActionMap: string, confirmationMessage?: string) {
        if (confirmationMessage) {
            if (confirm(confirmationMessage)) {
                return false;
            }
        }
        
        const currentTableActionInput = document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName);
        const currentFormActionInput = document.querySelector<HTMLInputElement>("#form-view-action-map-" + componentName);

        currentTableActionInput.value = null;
        currentFormActionInput.value = encryptedActionMap;
        
        let urlBuilder = new UrlBuilder()
        urlBuilder.addQueryParameter("context","modal")
        
        const url = urlBuilder.build()
        
        const modal = new Modal();
        modal.modalId = componentName +"-modal";
        modal.modalTitleId =  componentName +"-modal-tile";

        modal.showHtmlFromUrl(title, url, {
            method: "POST",
            body: new FormData(document.querySelector("form"))
        },1).then(_=>loadJJMasterData())
    }
}