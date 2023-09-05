class DataPanelHelper {
    
    static reload(componentName, fieldName, routeContext) {

        let urlBuilder = new UrlBuilder()
        urlBuilder.addQueryParameter("panelName",componentName)
        urlBuilder.addQueryParameter("componentName",fieldName)
        urlBuilder.addQueryParameter("routeContext",routeContext)
        
        const form = document.querySelector("form");
        fetch(urlBuilder.build(), {
            method: form.method,
            body: new FormData(form),
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(response.statusText);
                }
                return response.text();
            })
            .then(data => {
                document.getElementById(componentName).outerHTML = data;
                loadJJMasterData();
                jjutil.gotoNextFocus(fieldName);
            })
            .catch(error => {
                console.error(error);
            });
    }
}
