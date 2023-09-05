class ActionManager {

    static executeSqlCommand(componentName, rowId, confirmMessage) {
        if (confirmMessage) {
            var result = confirm(confirmMessage);
            if (!result) {
                return false;
            }
        }

        document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#grid-view-row-" + componentName).value = rowId;

        const formViewActionMapElement = document.querySelector<HTMLInputElement>("#form-view-action-map-" + componentName);

        if (formViewActionMapElement) {
            formViewActionMapElement.value = "";
        }

        document.querySelector("form").dispatchEvent(new Event("submit"));
    }

    static executeRedirectAction(componentName: string, routeContext: string, encryptedActionMap: string, confirmationMessage?: string) {
        if (confirmationMessage) {
            const result = confirm(confirmationMessage);
            if (!result) {
                return false;
            }
        }

        const currentFormActionInput = document.querySelector<HTMLInputElement>("#form-view-action-map-" + componentName);
        currentFormActionInput.value = encryptedActionMap;

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
            success: (data) => {
                if (data.urlAsPopUp) {
                    defaultModal.showIframe(data.urlRedirect, data.popUpTitle);
                } else {
                    window.location.href = data.urlRedirect;
                }
            }
        })
    }

    static executeAction(componentName: string, encryptedActionMap: string, routeContext: string = null, confirmationMessage: string = null, isModal: boolean = false) {
        if (confirmationMessage) {
            if (!confirm(confirmationMessage)) {
                return false;
            }
        }

        const gridViewActionInput = document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName);
        const formViewActionInput = document.querySelector<HTMLInputElement>("#form-view-action-map-" + componentName);

        if (gridViewActionInput) {
            gridViewActionInput.value = null;
        }
        if (formViewActionInput) {
            formViewActionInput.value = encryptedActionMap;
        }

        let form = document.querySelector<HTMLFormElement>("form");

        if (!form) {
            return;
        }

        if (isModal) {
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("routeContext", routeContext);

            const modal = new Modal();
            modal.modalId = componentName + "-modal";

            modal.showUrl({
                url: urlBuilder.build(), requestOptions: {
                    method: "POST",
                    body: new FormData(document.querySelector("form"))
                }
            }, componentName).then(function (data) {
                
                listenAllEvents("#" + modal.modalId + " ")    
                
                if (typeof data === "object") {
                    if (data.closeModal) {
                        modal.hide();
                        //GridViewHelper.refresh(componentName,"")
                    }
                }
            })
        } else {
            form.submit();
        }
    }

    static executeFormAction(componentName: string, encryptedActionMap: string, confirmationMessage: string) {
        this.executeAction(componentName, encryptedActionMap, null, confirmationMessage, false);
    }

    static executeModalAction(componentName: string, encryptedActionMap: string, routeContext: string, confirmationMessage: string) {
        this.executeAction(componentName, encryptedActionMap, routeContext, confirmationMessage, true);
    }
}