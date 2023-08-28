class DataPanel {
    static reloadAtSamePage(panelname, objid){
        let url = new UrlBuilder()
        url.addQueryParameter("panelName",panelname)
        url.addQueryParameter("componentName",objid)
        url.addQueryParameter("context","panelReload")

        DataPanel.reload(url.build(), panelname, objid)
    }
    
    static reload(url, componentName, fieldName) {
        const form = document.querySelector("form");
        fetch(url, {
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
