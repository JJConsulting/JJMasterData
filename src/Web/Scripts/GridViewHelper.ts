

class GridViewHelper {

    static openSettingsModal(componentName: string, encryptedActionMap: string) {
        const gridViewActionInput = document.getElementById("grid-view-action-map-" + componentName) as HTMLInputElement;
        const gridViewPageInput = document.getElementById("grid-view-page-" + componentName) as HTMLInputElement;
        const gridViewRowInput = document.getElementById("grid-view-row-" + componentName) as HTMLInputElement;
        const form = document.querySelector("form") as HTMLFormElement;

        if (gridViewActionInput && gridViewPageInput && gridViewRowInput && form) {
            gridViewActionInput.value = encryptedActionMap;
            gridViewPageInput.value = "1";
            gridViewRowInput.value = "";
            
            this.clearCurrentFormAction(componentName);
            
            form.requestSubmit();
        }
    }

    static closeSettingsModal(componentName: string) {
        const form = document.querySelector("form");
        const checkboxes = document.querySelectorAll("form");
        const modalId = "config-modal-" + componentName;
        const modalElement = document.getElementById("config-modal-" + componentName);

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

        if (modalElement) {
            const modal = new Modal();
            modal.modalId = modalId;
            modal.hide();
        }
    }
    
    static sortGridValues(componentName, routeContext, field) {
        const tableOrderElement = document.querySelector<HTMLInputElement>("#grid-view-order-" + componentName);
        if (field + " ASC" === tableOrderElement.value)
            tableOrderElement.value = field + " DESC";
        else
            tableOrderElement.value = field + " ASC";

        document.querySelector<HTMLInputElement>("#grid-view-action-map-" + componentName).value = "";
        this.clearCurrentFormAction(componentName)

        GridViewHelper.refreshGrid(componentName, routeContext);
    }

    static sortMultItems(componentName, routeContext) {
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
        
        let modal = new Modal();
        modal.modalId = componentName + "-sort-modal";
        modal.hide();
        
        this.clearCurrentFormAction(componentName);
        GridViewHelper.refreshGrid(componentName, routeContext);
    }


    static clearCurrentFormAction(componentName){
        const currentFormAction = document.querySelector<HTMLInputElement>("#form-view-action-map-" + componentName);
        
        if(currentFormAction)
            currentFormAction.value = "";
    }

    static setCurrentGridPage(componentName, currentPage){
        const currentGridPage = document.querySelector<HTMLInputElement>("#grid-view-page-" + componentName);

        if(currentGridPage)
            currentGridPage.value = currentPage;
    }

    static clearCurrentGridAction(componentName){
        const currentGridAction = document.querySelector<HTMLInputElement>("#grid-view-action-map-" + componentName);

        if(currentGridAction)
            currentGridAction.value = "";
    }
    
    static paginate(componentName, routeContext, currentPage: number) {
        this.setCurrentGridPage(componentName,currentPage);
        this.clearCurrentGridAction(componentName)
        this.clearCurrentFormAction(componentName)

        GridViewHelper.refreshGrid(componentName, routeContext);
    }
    
    static jumpToPage(componentName, routeContext) {
        const jumpToPageInput = document.querySelector<HTMLInputElement>("#" + componentName + "-jump-to-page-input");

        const numericPage = Number(jumpToPageInput.value)
        
        if(isNaN(numericPage) || numericPage <= 0 || numericPage > Number(jumpToPageInput.max)){
            jumpToPageInput.classList.add("is-invalid")
            return;
        }
        
        this.paginate(componentName,routeContext, numericPage)
    }
    
    static showJumpToPage(jumpToPageName: string){
        const jumpToPageInput = $("#" + jumpToPageName);
        
        jumpToPageInput.val(null);
        
        jumpToPageInput.animate({width: 'toggle'},null, function(){
            jumpToPageInput.removeClass("is-invalid")
        });
    }
    
    static refresh(componentName: string, routeContext: string) {
        this.setCurrentGridPage(componentName,String());
        this.clearCurrentGridAction(componentName)
        this.clearCurrentFormAction(componentName)
        GridViewHelper.refreshGrid(componentName, routeContext);
    }

    static refreshGrid(componentName: string, routeContext: string) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);

        postFormValues({
            url: urlBuilder.build(),
            success: function (data) {
                const gridViewTableElement = document.querySelector<HTMLInputElement>("#grid-view-table-" + componentName);
                const filterActionElement = document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName);

                if (gridViewTableElement) {
                    gridViewTableElement.outerHTML = data;
                    
                    listenAllEvents("#" + componentName);
                    
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