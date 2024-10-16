class GridViewFilterHelper {
    static filter(gridViewName, routeContext) {
        document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + gridViewName).value = "filter";
        document.querySelector<HTMLInputElement>("#grid-view-action-map-" + gridViewName).value = "";
        document.querySelector<HTMLInputElement>("#grid-view-page-" + gridViewName).value = "1";
        
        GridViewHelper.clearCurrentFormAction(gridViewName);
        GridViewHelper.refreshGrid(gridViewName, routeContext);

        this.showFilterIcon(gridViewName);
    }

    static showFilterIcon(gridViewName: string){
        const filterIcon = document.getElementById(gridViewName + "-filter-icon");
        new bootstrap.Tooltip(filterIcon, { trigger: 'hover' });
        filterIcon.classList.remove("d-none");
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
        
        $(selector).each(function () {
            let currentObj = $(this);
            
            if (currentObj.hasClass("flatpickr-input")) {
                currentObj.val("")
            }

            if(currentObj.selectpicker){
                currentObj.selectpicker("val","");
            }

            if(currentObj.typeahead){
                currentObj.typeahead("val","");
                currentObj.typeahead("destroy");
            }

            if(currentObj.hasClass("jj-numeric")){
                //@ts-ignore
                const autoNumeric = AutoNumeric.getAutoNumericElement(currentObj[0])
                autoNumeric.clear();
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
                } else if (currentObj.hasClass("jj-search-box")) {
                    currentObj.blur();
                } else if (currentObj.hasClass("jjlookup")) {
                    currentObj.blur();
                }
            }
        });

        document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName).value = "clear";
        document.querySelector<HTMLInputElement>("#grid-view-action-map-" + componentName).value = "";
        GridViewHelper.clearCurrentFormAction(componentName)
    }

    static clearFilter(componentName, routeContext) {
        this.clearFilterInputs(componentName);

        GridViewHelper.refreshGrid(componentName, routeContext);

        document.getElementById(componentName + "-filter-icon").classList.add("d-none");
    }

    static searchOnDOM(componentName, oDom) {
        const value = $(oDom).val().toString().toLowerCase();
        $("#" + componentName + "-table" + " tr").filter(<any>function () {
            //procura por textos
            const textValues = $(this).clone().find('.bootstrap-select, .selectpicker, select').remove().end().text();
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