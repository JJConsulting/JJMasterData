class ActionManager {
    static executePanelAction(name: string, action: string){
        $("#current_painelaction_" + name).val(action);
        let form = document.forms[name];

        if(!form){
            form = document.forms[0];
        }
        
        form.submit()
        return false;
    }
    
    static executeFormAction(actionName: string, encryptedActionMap: string, confirmationMessage?: string) {
        if (confirmationMessage) {
            if (confirm(confirmationMessage)) {
                return false;
            }
        }

        const currentTableActionInput = document.querySelector<HTMLInputElement>("#current_tableaction_" + actionName);
        const currentFormActionInput = document.querySelector<HTMLInputElement>("#current_formaction_" + actionName);
        
        let form = document.querySelector<HTMLFormElement>("form");

        if(!form){
            form = document.forms[0];
        }
        
        currentTableActionInput.value = "";
        currentFormActionInput.value = encryptedActionMap;
        form.submit();
    }

    static executeFormActionAsPopUp(actionName: string, encryptedActionMap: string, confirmationMessage?: string) {
        if (confirmationMessage) {
            if (confirm(confirmationMessage)) {
                return false;
            }
        }
        
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("t","popup")

        const formData = new URLSearchParams();
        formData.append("current_formaction_" + actionName, encryptedActionMap);
        
        popup.showHtmlFromUrl("Teste", urlBuilder.build(), {
            method: "POST",
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: formData
        }, 4);
    }
}
