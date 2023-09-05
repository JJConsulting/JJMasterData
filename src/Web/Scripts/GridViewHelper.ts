class GridViewHelper {

    static openSettingsModal(componentName: string, encryptedActionMap: string) {
        const gridViewActionInput = document.getElementById("grid-view-action-" + componentName) as HTMLInputElement;
        const gridViewPageInput = document.getElementById("grid-view-page-" + componentName) as HTMLInputElement;
        const gridViewRowInput = document.getElementById("grid-view-row-" + componentName) as HTMLInputElement;
        const form = document.querySelector("form") as HTMLFormElement;

        if (gridViewActionInput && gridViewPageInput && gridViewRowInput && form) {
            gridViewActionInput.value = encryptedActionMap;
            gridViewPageInput.value = "1";
            gridViewRowInput.value = "";
            
            this.clearCurrentFormAction(componentName);
            
            form.dispatchEvent(new Event("submit", { bubbles: true, cancelable: true }));
        }
    }

    static closeSettingsModal(componentName: string) {
        const form = document.querySelector("form");
        const checkboxes = document.querySelectorAll("form");
        const modal = document.getElementById("config-modal-" + componentName);

        if (form) {
            form.reset();
        }

        if (checkboxes) {
            checkboxes.forEach((checkbox) => {
                if (checkbox instanceof HTMLInputElement) {
                    checkbox.dispatchEvent(new Event("change", { bubbles: true, cancelable: true }));
                }
            });
        }

        if (modal) {
            modal.classList.remove("show");
            modal.style.display = "none";
        }
    }
    
    static sortGridValues(componentName, routeContext, field) {
        const tableOrderElement = document.querySelector<HTMLInputElement>("#grid-view-order-" + componentName);
        if (field + " ASC" === tableOrderElement.value)
            tableOrderElement.value = field + " DESC";
        else
            tableOrderElement.value = field + " ASC";

        document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName).value = "";
        this.clearCurrentFormAction(componentName)

        GridViewHelper.refreshGrid(componentName, routeContext);
    }

    static sortItems(componentName) {
        var descCommand = "";

        // @ts-ignore
        var order = $("#sortable-" + componentName).sortable("toArray");

        for (var i = 0; i < order.length; i++) {
            var sortingType = $("#" + order[i] + "_order").children("option:selected").val();
            switch (sortingType) {
                case "A":
                    descCommand += order[i] + " ASC,";
                    break;
                case "D":
                    descCommand += order[i] + " DESC,";
                    break;
            }
        }
        descCommand = descCommand.substring(0, descCommand.length - 1);
        
        document.querySelector<HTMLInputElement>("#grid-view-order-" + componentName).value = descCommand;
        
        $("#sort-modal-" + componentName).modal('hide');
        
        this.clearCurrentFormAction(componentName);
    }


    static clearCurrentFormAction(componentName){
        const currentFormAction = document.querySelector<HTMLInputElement>("#form-view-action-map-" + componentName);
        
        if(currentFormAction)
            currentFormAction.value = "";
    }

    static paginate(componentName, routeContext, currentPage) {
        document.querySelector<HTMLInputElement>("#grid-view-page-" + componentName).value = currentPage;
        document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName).value = "";
        this.clearCurrentFormAction(componentName)

        GridViewHelper.refreshGrid(componentName, routeContext);
    }


    static refresh(componentName: string, routeContext: string) {
        document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#grid-view-row-" + componentName).value = "";
        this.clearCurrentFormAction(componentName)
        GridViewHelper.refreshGrid(componentName, routeContext);
    }

    static refreshGrid(componentName: string, routeContext: string, reloadListeners = false) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);

        postFormValues({
            url: urlBuilder.build(),
            success: function (data) {
                const gridViewElement = document.querySelector<HTMLInputElement>("#grid-view-" + componentName);
                const filterActionElement = document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName);

                if (gridViewElement) {
                    gridViewElement.innerHTML = data;
                    if (reloadListeners) {
                        listenAllEvents();
                    }
                    if(filterActionElement){
                        filterActionElement.value = "";
                    }
                } else {
                    console.error("One or both of the elements were not found.");
                }
            },
            error: function (error) {
                console.error(error);
                const filterActionElement = document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName);

                if (filterActionElement) {
                    filterActionElement.value = "";
                } else {
                    console.error("Filter action element was not found.");
                }
            }
        });
    }
}