

class GridViewHelper {

    static setGridSettings(componentName: string, encryptedRouteContext: string,encryptedActionMap: string) {
        const gridViewActionInput = document.getElementById("grid-view-action-map-" + componentName) as HTMLInputElement;
        const gridViewPageInput = document.getElementById("grid-view-page-" + componentName) as HTMLInputElement;
        const gridViewRowInput = document.getElementById("grid-view-row-" + componentName) as HTMLInputElement;

        if (gridViewActionInput && gridViewPageInput && gridViewRowInput) {
            gridViewActionInput.value = encryptedActionMap;
            gridViewPageInput.value = "1";
            gridViewRowInput.value = "";
            
            this.clearCurrentFormAction(componentName);
            
            this.closeSettingsModal(componentName, false);
            
            GridViewHelper.refreshGrid(componentName, encryptedRouteContext);
        }
    }

    static closeSettingsModal(componentName: string, clearFormValues: boolean = true) {
        
        const modalId = "config-modal-" + componentName;
        const checkboxes = document.querySelectorAll("#" + modalId + " input");
        const modalElement = document.getElementById("config-modal-" + componentName);

        if (clearFormValues) {
            const form = getMasterDataForm();
            form?.reset();
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
        const currentFormAction = document.querySelector<HTMLInputElement>("#current-action-map-" + componentName);
        
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

    static scroll(componentName, routeContext, currentPage: number) {

        
        this.setCurrentGridPage(componentName,currentPage);
        this.clearCurrentGridAction(componentName)
        this.clearCurrentFormAction(componentName)

        GridViewHelper.appendGridRows(componentName, routeContext);
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
    
    
    /**
     * Submits the grid form for the specified component.
     *
     * @param {string} componentName - The name of the component.
     * @return {void}
     */
    static submitGrid(componentName: string){
        this.setCurrentGridPage(componentName,String());
        this.clearCurrentGridAction(componentName)
        this.clearCurrentFormAction(componentName)
        getMasterDataForm().submit();
    }

    static appendGridRows(componentName: string, routeContext: string) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        SpinnerOverlay.visible = false;
        const gridViewTableElement = document.querySelector<HTMLInputElement>("#grid-view-table-" + componentName);
        const tbody = gridViewTableElement.querySelector("tbody")
        const placeholders = tbody.querySelectorAll(".md-tr-placeholder");
        placeholders.forEach(placeholder => placeholder.classList.remove("d-none"));

        postFormValues({
            url: urlBuilder.build(),
            success: function (data) {
                const filterActionElement = document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName);

                if (gridViewTableElement) {
                    console.log("opa");
                    console.log(data);
                    TooltipHelper.dispose("#" + componentName)
                  
                    const target = placeholders[0];

                    if (target) {
                        target.insertAdjacentHTML('beforebegin', data);
                    }
                    placeholders.forEach(placeholder => placeholder.classList.add("d-none"));
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
                }
            }
        });
        SpinnerOverlay.visible = true;
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
                    TooltipHelper.dispose("#" + componentName)
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
                }
            }
        });
    }
    
    static reloadGridRow(componentName, fieldName , gridViewRowIndex, routeContext){
        const urlBuilder = new UrlBuilder()
        urlBuilder.addQueryParameter("gridViewName",componentName)
        urlBuilder.addQueryParameter("gridViewRowIndex", gridViewRowIndex)
        urlBuilder.addQueryParameter("routeContext",routeContext)

        const fieldId = document.querySelector(`input[gridviewrowindex="${gridViewRowIndex}"].${fieldName}, select[gridviewrowindex="${gridViewRowIndex}"].${fieldName}`).id;        
        postFormValues({
            url: urlBuilder.build(),
            success: data => {
                $("#" + componentName + " #row" + gridViewRowIndex).html(data);
                listenAllEvents("#" + componentName);
                
                jjutil.gotoNextFocus(fieldId);
            }
        });
    }

    static setupInfiniteScroll(selectorPrefix: string) {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    observer.unobserve(entry.target);
                    entry.target.setAttribute("observed", "true");

                    const gridName = entry.target.getAttribute("grid-name")
                    const routeContext = entry.target.getAttribute("grid-pagination-route-context")
                    const nextPage = entry.target.getAttribute("grid-pagination-next-page")
                    GridViewHelper.scroll(gridName,routeContext, Number(nextPage));
                }
            });
        });

        document.querySelectorAll(selectorPrefix + '.grid-pagination-last-row').forEach(el => {
            if(!el.getAttribute("observed")){
                observer.observe(el);
            }
        });
    }
}