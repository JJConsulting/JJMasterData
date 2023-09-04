class GridViewFilterHelper{
    static filter(componentName, url) {
        document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName).value = "FILTERACTION";
        document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName).value = "";
        document.querySelector<HTMLInputElement>("#grid-view-page-" + componentName).value = "1";
        GridViewHelper.clearCurrentFormAction(componentName)
        GridViewHelper.refreshGrid(componentName, url);
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
                } else if (currentObj.hasClass("jj-search-box")) {
                    currentObj.blur();
                } else if (currentObj.hasClass("jjlookup")) {
                    currentObj.blur();
                }
            }
        });

        document.querySelector<HTMLInputElement>("#grid-view-filter-action-" + componentName).value = "CLEARACTION";
        document.querySelector<HTMLInputElement>("#grid-view-action-" + componentName).value = "";
        GridViewHelper.clearCurrentFormAction(componentName)
    }
    static clearFilter(componentName, url) {
        this.clearFilterInputs(componentName);

        GridViewHelper.refreshGrid(componentName, url);
    }

    static searchOnDOM(objid, oDom) {
        var value = $(oDom).val().toString().toLowerCase();
        $("#table_" + objid + " tr").filter(<any>function () {
            //procura por textos
            var textValues = $(this).clone().find('.bootstrap-select, .selectpicker, select').remove().end().text();
            var isSearch = textValues.toLowerCase().indexOf(value) > -1;

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
            $("#infotext_" + objid).css("display", "none");
            $("ul.pagination").css("display", "none");
        } else {
            $("#infotext_" + objid).css("display", "");
            $("ul.pagination").css("display", "");
        }
    }
}