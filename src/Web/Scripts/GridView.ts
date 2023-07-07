class GridView {
    static sorting(componentName, url, tableOrder) {
        const tableOrderElement = document.querySelector<HTMLInputElement>("#current_tableorder_" + componentName);
        if (tableOrder + " ASC" === tableOrderElement.value)
            tableOrderElement.value = tableOrder + " DESC";
        else
            tableOrderElement.value = tableOrder + " ASC";

        document.querySelector<HTMLInputElement>("#current_tableaction_" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#current_formaction_" + componentName).value = "";

        GridView.refreshGrid(componentName, url);
    }

    static pagination(componentName, url, currentPage) {
        document.querySelector<HTMLInputElement>("#current_tablepage_" + componentName).value = currentPage;
        document.querySelector<HTMLInputElement>("#current_tableaction_" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#current_formaction_" + componentName).value = "";

        GridView.refreshGrid(componentName, url);
    }

    static filter(componentName, url) {
        document.querySelector<HTMLInputElement>("#current_filteraction_" + componentName).value = "FILTERACTION";
        document.querySelector<HTMLInputElement>("#current_tableaction_" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#current_tablepage_" + componentName).value = "1";
        document.querySelector<HTMLInputElement>("#current_formaction_" + componentName).value = "";
        GridView.refreshGrid(componentName, url);
    }

    static refresh(componentName, url) {
        document.querySelector<HTMLInputElement>("#current_tableaction_" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#current_tablerow_" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#current_formaction_" + componentName).value = "";
        GridView.refreshGrid(componentName, url);
    }

    static refreshGrid(componentName, url) {
        const form = document.querySelector("form");
        
        let urlBuilder = new UrlBuilder(url)
        
        urlBuilder.addQueryParameter("componentName",componentName)
        
        fetch(urlBuilder.build(), {
            method: form.method,
            body: new FormData(form)
        })
            .then(response => response.text())
            .then(data => {
                document.querySelector<HTMLInputElement>("#jjgridview_" + componentName).innerHTML = data;
                jjloadform();
                document.querySelector<HTMLInputElement>("#current_filteraction_" + componentName).value = "";
            })
            .catch(error => {
                console.log(error);
                document.querySelector<HTMLInputElement>("#current_filteraction_" + componentName).value = "";
            });
    }
}