class DataPanelHelper {
    public static reload(panelName, elementFieldName, fieldNameWithPrefix, routeContext) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("panelName", panelName);
        urlBuilder.addQueryParameter("fieldName", elementFieldName);
        urlBuilder.addQueryParameter("routeContext", routeContext);
        
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
                jjutil.gotoNextFocus(fieldNameWithPrefix);
            }
        });
    }
}
