
class ActionManager {
    static executePanelAction(name: string, action: string){
        $("#current-panel-action-" + name).val(action);
        let form = document.querySelector<HTMLFormElement>(`form#${name}`);

        if(!form){
            form = document.forms[0];
        }

        form.requestSubmit()
        return false;
    }

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

        const currentFormActionInput = document.querySelector<HTMLInputElement>("#current-form-action-" + componentName);
        currentFormActionInput.value = encryptedActionMap;

        if(!url){
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("t", "geturlaction");
            urlBuilder.addQueryParameter("objname", componentName);

            url = urlBuilder.build();
        }

        this.executeUrlRedirect(url);

        return true;
    }

    private static executeUrlRedirect(url: string) {
        fetch(url, {
            method: "POST",
            body: new FormData(document.querySelector<HTMLFormElement>("form"))
        }).then(response => response.json()).then(data => {
            if (data.urlAsPopUp) {
                popup.show(data.popUpTitle, data.urlRedirect);
            } else {
                window.location.href = data.urlRedirect;
            }
        })
    }

    static executeFormAction(componentName: string, encryptedActionMap: string, confirmationMessage?: string) {
        if (confirmationMessage) {
            if (confirm(confirmationMessage)) {
                return false;
            }
        }

        const currentTableActionInput = document.querySelector<HTMLInputElement>("#current-table-action-" + componentName);
        const currentFormActionInput = document.querySelector<HTMLInputElement>("#current-form-action-" + componentName);

        let form = document.querySelector<HTMLFormElement>("form");

        if(!form){
            form = document.forms[0];
        }

        currentTableActionInput.value = "";
        currentFormActionInput.value = encryptedActionMap;
        
        form.submit();
    }

    static executeFormActionAsPopUp(url:string, title: string, confirmationMessage?: string) {
        if (confirmationMessage) {
            if (confirm(confirmationMessage)) {
                return false;
            }
        }
        
        popup.showHtmlFromUrl(title, url, {
            method: "POST",
            body: new FormData(document.querySelector("form"))
        },1).then(_=>loadJJMasterData())
    }
}