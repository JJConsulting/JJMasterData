
class ActionManager {
    static executePanelAction(name: string, action: string){
        $("#current-panelAction-" + name).val(action);
        let form = document.querySelector<HTMLFormElement>(`form#${name}`);

        if(!form){
            form = document.forms[0];
        }

        form.requestSubmit()
        return false;
    }

    static executeRedirectAction(url: string, componentName: string, encryptedActionMap: string, confirmMessage?: string): boolean;
    static executeRedirectAction(componentName: string, encryptedActionMap: string, confirmMessage?: string): boolean;
    static executeRedirectAction(...args: any[]): boolean {
        let url = args[0];
        const componentName = args[1];
        const encryptedActionMap = args[2];
        const confirmMessage = args[3];

        if (confirmMessage) {
            const result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }

        const currentFormActionInput = document.querySelector<HTMLInputElement>("#current-formAction-" + componentName);
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

        const currentTableActionInput = document.querySelector<HTMLInputElement>("#current-tableAction-" + componentName);
        const currentFormActionInput = document.querySelector<HTMLInputElement>("#current-formAction-" + componentName);

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
        },1).then(_=>loadJJMasterData())
    }
}