class GridView {
    static sorting(componentName, url, tableOrder) {
        const tableOrderElement = document.querySelector<HTMLInputElement>("#grid-view-order-" + componentName);
        if (tableOrder + " ASC" === tableOrderElement.value)
            tableOrderElement.value = tableOrder + " DESC";
        else
            tableOrderElement.value = tableOrder + " ASC";

        document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName).value = "";
        this.clearCurrentFormAction(componentName)

        GridView.refreshGrid(componentName, url);
    }
    
    static clearCurrentFormAction(componentName){
        const currentFormAction = document.querySelector<HTMLInputElement>("#form-view-action-map-" + componentName);
        
        if(currentFormAction)
            currentFormAction.value = "";
    }

    static pagination(componentName, url, currentPage) {
        document.querySelector<HTMLInputElement>("#grid-view-page-" + componentName).value = currentPage;
        document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName).value = "";
        this.clearCurrentFormAction(componentName)

        GridView.refreshGrid(componentName, url);
    }

    static filter(componentName, url) {
        document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName).value = "FILTERACTION";
        document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#grid-view-page-" + componentName).value = "1";
        this.clearCurrentFormAction(componentName)
        GridView.refreshGrid(componentName, url);
    }

    
    static clearFilterInputs(componentName){
        const divId = "#current-grid-filter-" + componentName;
        const selector = divId + " input:enabled, " + divId + " select:enabled";


        $(selector).each(function () {
            let currentObj = $(this);

            if (currentObj.hasClass("flatpickr-input")) {
                currentObj.val("")
            }
            let inputType: string = (this as any).type;

            if (inputType == "checkbox") {
                currentObj.prop("checked", false);
            } else if (inputType != "input" && currentObj.attr("data-role") == "tagsinput") {
                currentObj.tagsinput('removeAll');
            } else if (inputType != "hidden") {
                currentObj.val("");
                if (currentObj.hasClass("selectpicker")) {
                    currentObj.selectpicker("render");
                } else if (currentObj.hasClass("jjsearchbox")) {
                    currentObj.blur();
                } else if (currentObj.hasClass("jjlookup")) {
                    currentObj.blur();
                }
            }
        });

        document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName).value = "CLEARACTION";
        document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName).value = "";
        this.clearCurrentFormAction(componentName)
    }
    static clearFilter(componentName, url) {
        this.clearFilterInputs(componentName);
        
        GridView.refreshGrid(componentName, url);
    }


    static refresh(componentName, url) {
        document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#grid-view-row-" + componentName).value = "";
        this.clearCurrentFormAction(componentName)
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

        const selectedRowsInput = document.getElementById("grid-view-selected-rows" + componentName) as HTMLInputElement;
        selectedRowsInput.value = values.join(",");

        const selectedText = document.getElementById("selected-text-" + componentName);
        selectedText.textContent = selectedText.getAttribute("multiple-records-selected-label").replace("{0}", values.length.toString());
    }

    static refreshGrid(componentName, url) {
        const form = document.querySelector("form");

        let urlBuilder = new UrlBuilder(url)

        urlBuilder.addQueryParameter("componentName", componentName)

        SpinnerOverlay.show()

        const filterAction = document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName);
        
        fetch(urlBuilder.build(), {
            method: form.method,
            body: new FormData(form)
        })
            .then(response => response.text())
            .then(data => {
                document.querySelector<HTMLInputElement>("#grid-view-" + componentName).innerHTML = data;
                loadJJMasterData();
                
                if(filterAction)
                    filterAction.value = "";
                
                SpinnerOverlay.hide();
            })
            .catch(error => {
                console.log(error);
                if(filterAction)
                    filterAction.value = "";
            });
    }
}