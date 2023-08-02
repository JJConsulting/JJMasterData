class DataPanel {
    static ReloadAtSamePage(panelname, objid){
        let url = new UrlBuilder()
        url.addQueryParameter("pnlname",panelname)
        url.addQueryParameter("objname",objid)
        url.addQueryParameter("t","reloadPanel")

        DataPanel.Reload(url.build(), panelname, objid)
    }
    
    static Reload(url, componentName, fieldName) {
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
                jjloadform();
                jjutil.gotoNextFocus(fieldName);
            })
            .catch(error => {
                console.error(error);
            });
    }
}
