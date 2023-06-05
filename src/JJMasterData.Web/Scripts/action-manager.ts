class ActionManager {
    static executeFormAction(actionName: string, encryptedActionMap: string, confirmationMessage?: string) {
        if (confirmationMessage) {
            if (confirm(confirmationMessage)) {
                return false;
            }
        }

        const currentTableActionInput = document.querySelector<HTMLInputElement>("#current_tableaction_" + actionName);
        const currentFormActionInput = document.querySelector<HTMLInputElement>("#current_formaction_" + actionName);
        
        const form = document.querySelector<HTMLFormElement>("form");

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
