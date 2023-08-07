class GridView {
    static sorting(componentName, url, tableOrder) {
        const tableOrderElement = document.querySelector<HTMLInputElement>("#current-table-order-" + componentName);
        if (tableOrder + " ASC" === tableOrderElement.value)
            tableOrderElement.value = tableOrder + " DESC";
        else
            tableOrderElement.value = tableOrder + " ASC";

        document.querySelector<HTMLInputElement>("#current-table-action-" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#current-form-action-" + componentName).value = "";

        GridView.refreshGrid(componentName, url);
    }

    static pagination(componentName, url, currentPage) {
        document.querySelector<HTMLInputElement>("#current-table-page-" + componentName).value = currentPage;
        document.querySelector<HTMLInputElement>("#current-table-action-" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#current-form-action-" + componentName).value = "";

        GridView.refreshGrid(componentName, url);
    }

    static filter(componentName, url) {
        document.querySelector<HTMLInputElement>("#current-filter-action-" + componentName).value = "FILTERACTION";
        document.querySelector<HTMLInputElement>("#current-table-action-" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#current-table-page-" + componentName).value = "1";
        document.querySelector<HTMLInputElement>("#current-form-action-" + componentName).value = "";
        GridView.refreshGrid(componentName, url);
    }

    static refresh(componentName, url) {
        document.querySelector<HTMLInputElement>("#current-table-action-" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#current-table-row-" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#current-form-action-" + componentName).value = "";
        GridView.refreshGrid(componentName, url);
    }

    static selectAllRows(componentName, url) {
        fetch(url, {method:"POST"})
            .then(response => response.json())
            .then(data => GridView.selectAllRowsElements(componentName, data.selectedRows))
    }

    static selectAllRowsElements(componentName, rows) {
        const values = rows.split(",");

        const checkboxes = document.querySelectorAll<HTMLInputElement>(".jjselect input:not(:disabled)");
        checkboxes.forEach(checkbox => checkbox.checked = true);

        const selectedRowsInput = document.getElementById("selected-rows" + componentName) as HTMLInputElement;
        selectedRowsInput.value = values.join(",");

        const selectedText = document.getElementById("selected-text-" + componentName);
        selectedText.textContent = selectedText.getAttribute("multiple-records-selected-label").replace("{0}", values.length.toString());
    }

    static refreshGrid(componentName, url) {
        const form = document.querySelector("form");

        let urlBuilder = new UrlBuilder(url)

        urlBuilder.addQueryParameter("componentName", componentName)

        SpinnerOverlay.show()
        
        fetch(urlBuilder.build(), {
            method: form.method,
            body: new FormData(form)
        })
            .then(response => response.text())
            .then(data => {
                document.querySelector<HTMLInputElement>("#jjgridview-" + componentName).innerHTML = data;
                loadJJMasterData();
                document.querySelector<HTMLInputElement>("#current-filter-action-" + componentName).value = "";
                SpinnerOverlay.hide();
            })
            .catch(error => {
                console.log(error);
                document.querySelector<HTMLInputElement>("#current-filter-action-" + componentName).value = "";
            });
    }
}