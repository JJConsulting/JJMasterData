class DataPanelHelper {
    static reload(panelName, elementFieldName, fieldNameWithPrefix, routeContext) {
        this.reloadInternal(panelName, elementFieldName, fieldNameWithPrefix, routeContext);
    }

    static reloadWithTimeout(panelName, elementFieldName, fieldNameWithPrefix, routeContext) {
        setTimeout(() => {
            this.reloadInternal(panelName, elementFieldName, fieldNameWithPrefix, routeContext);
        }, 200);
    }

    private static reloadInternal(panelName, elementFieldName, fieldNameWithPrefix, routeContext) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("panelName", panelName);
        urlBuilder.addQueryParameter("fieldName", elementFieldName);
        urlBuilder.addQueryParameter("routeContext", routeContext);
        
        const originalElement = document.getElementById(fieldNameWithPrefix) as HTMLInputElement;

        const selectionStart = originalElement ? originalElement.selectionStart : 0;
        const selectionEnd = originalElement ? originalElement.selectionEnd : 0;

        postFormValues({
            url: urlBuilder.build(),
            success: data => {
                if (typeof data === "string") {
                    HTMLHelper.setOuterHTML(panelName, data);
                    listenAllEvents("#" + panelName);
                } else {
                    if (data.jsCallback) {
                        eval(data.jsCallback);
                    }
                }

                const fieldElement = document.getElementById(fieldNameWithPrefix) as HTMLInputElement;

                if (fieldElement.onchange) {
                    jjutil.gotoNextFocus(fieldNameWithPrefix);
                } else {
                    fieldElement.focus();
                    fieldElement.selectionStart = selectionStart;
                    fieldElement.selectionEnd = selectionEnd;
                }
            }
        });
    }
}
