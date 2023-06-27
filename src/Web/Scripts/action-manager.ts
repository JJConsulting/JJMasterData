
class ActionManager {
    static executePanelAction(name: string, action: string){
        $("#current_painelaction_" + name).val(action);
        let form = document.querySelector<HTMLFormElement>(`form#${name}`);

        if(!form){
            form = document.forms[0];
        }

        form.requestSubmit()
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

    static executeFormActionAsPopUp(url:string, title: string, confirmationMessage?: string) {
        if (confirmationMessage) {
            if (confirm(confirmationMessage)) {
                return false;
            }
        }
        
        popup.showHtmlFromUrl(title, url, {
            method: "POST",
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body:JSON.stringify({})
        },1).then(_=>jjloadform())
    }
}