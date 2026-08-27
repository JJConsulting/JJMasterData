class GridViewFilterHelper {
    static filter(gridViewName, routeContext, isSubmit) {
        document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + gridViewName).value = "filter";
        document.querySelector<HTMLInputElement>("#grid-view-action-map-" + gridViewName).value = "";
        document.querySelector<HTMLInputElement>("#grid-view-page-" + gridViewName).value = "1";
        
        GridViewHelper.clearCurrentFormAction(gridViewName);

        if(isSubmit) {
            getMasterDataForm().submit();
        }
        else{
            GridViewHelper.refreshGrid(gridViewName, routeContext);
            this.showFilterIcon(gridViewName);
        }
    }

    static showFilterIcon(gridViewName: string){
        const filterIcon = document.getElementById(gridViewName + "-filter-icon");
        filterIcon.classList.remove("d-none");
        
        if (bootstrapVersion !== 3) {
            bootstrap.Tooltip.getOrCreateInstance(filterIcon, {trigger:"click hover focus"});
        }
    }
    
    static reload(gridViewName, filterPanelName, routeContext) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext);
        
        document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + gridViewName).value = "filter";
        
        postFormValues({
            url: urlBuilder.build(), 
            success: (content) => {
                HTMLHelper.setOuterHTML(filterPanelName, content)
                listenAllEvents("#" + filterPanelName);
            }
        })
    }

    static clearFilterInputs(componentName) {
        const divId = "#current-grid-filter-" + componentName;
        const selector = divId + " input:enabled, " + divId + " select:enabled";
        const inputNames = Array.from(document.querySelectorAll<HTMLInputElement | HTMLSelectElement>(selector))
            .map(input => input.name)
            .filter(Boolean);

        // Remove the filter values from the next request without changing the visible inputs.
        getMasterDataForm().addEventListener("formdata", (event: FormDataEvent) => {
            inputNames.forEach(name => event.formData.delete(name));
        }, {once: true});
    }

    static clearFilter(componentName, routeContext, isSubmit, filterPanelName = null, filterRouteContext = null) {
        document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName).value = "clear";
        document.querySelector<HTMLInputElement>("#grid-view-action-map-" + componentName).value = "";
        GridViewHelper.clearCurrentFormAction(componentName);
        this.clearFilterInputs(componentName);

        if(isSubmit) {
            getMasterDataForm().submit();
            return;
        }

        GridViewHelper.setCurrentGridPage(componentName, 1)

        if(filterPanelName && filterRouteContext) {
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("routeContext", filterRouteContext);
            
            postFormValues({
                url: urlBuilder.build(),
                success: (content) => {
                    HTMLHelper.setOuterHTML(filterPanelName, content);
                    listenAllEvents("#" + filterPanelName);
                    document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName).value = "clear";
                    GridViewHelper.refreshGrid(componentName, routeContext);
                    document.getElementById(componentName + "-filter-icon").classList.add("d-none");
                }
            });
        }
        else {
            GridViewHelper.refreshGrid(componentName, routeContext);
            document.getElementById(componentName + "-filter-icon").classList.add("d-none");
        }
    }

    static searchOnDOM(componentName, oDom) {
        const value = $(oDom).val().toString().toLowerCase();
        $("#" + componentName + "-table" + " tr").filter(<any>function () {
            //procura por textos
            const textValues = $(this).clone().find('.ts-wrapper, .tom-select, select').remove().end().text();
            let isSearch = textValues.toLowerCase().indexOf(value) > -1;

            //se não achou procura nos inputs
            if (!isSearch) {
                var valueNew = value.replace(",", "").replace(".", "").replace("-", "");
                $(this).find("input").each(function () {
                    var inputValue = $(this).val();
                    if (inputValue != null) {
                        let isSearch = inputValue.toString().replace(",", "")
                            .replace(".", "")
                            .replace("-", "")
                            .toLowerCase()
                            .indexOf(valueNew) > -1;
                        if (isSearch)
                            return false;
                    }
                });
            }

            //se não achou procura nas combos
            if (!isSearch) {
                $(this).find("select").each(function () {
                    var selectedText = $(this).children("option:selected").text();
                    if (selectedText != null) {
                        isSearch = selectedText.toLowerCase().indexOf(valueNew) > -1;
                        if (isSearch)
                            return false;
                    }
                });
            }

            $(this).toggle(isSearch);
        });

        if (value.length > 0) {
            $("#infotext_" + componentName).css("display", "none");
            $("ul.pagination").css("display", "none");
        } else {
            $("#infotext_" + componentName).css("display", "");
            $("ul.pagination").css("display", "");
        }
    }
}
