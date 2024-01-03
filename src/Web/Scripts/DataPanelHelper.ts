class DataPanelHelper {
    
    static reload(componentName, fieldName, routeContext) {

        let urlBuilder = new UrlBuilder()
        urlBuilder.addQueryParameter("panelName",componentName)
        urlBuilder.addQueryParameter("fieldName",fieldName)
        urlBuilder.addQueryParameter("routeContext",routeContext)
        
        postFormValues({
            url: urlBuilder.build(),
            success: data => {
                if(typeof data === "string"){
                    HTMLHelper.setOuterHTML(componentName, data)
                    listenAllEvents("#" + componentName);
                    jjutil.gotoNextFocus(fieldName);
                }
                else{
                    if(data.jsCallback){
                        eval(data.jsCallback)
                    }
                }
            }
        })
    }
}
