class DataPanelHelper {
    
    static reload(componentName, fieldName, routeContext) {

        const urlBuilder = new UrlBuilder()
        urlBuilder.addQueryParameter("panelName",componentName)
        urlBuilder.addQueryParameter("fieldName",fieldName)
        urlBuilder.addQueryParameter("routeContext",routeContext)
        
        postFormValues({
            url: urlBuilder.build(),
            success: data => {
                if(typeof data === "string"){
                    HTMLHelper.setOuterHTML(componentName, data);
                    listenAllEvents("#" + componentName);
                }
                else{
                    if(data.jsCallback){
                        eval(data.jsCallback)
                    }
                }
                jjutil.gotoNextFocus(fieldName);
            }
        })
    }
}
