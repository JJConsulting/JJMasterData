class GridViewSelectionHelper{
    static selectItem(componentName: string, obj: HTMLInputElement) {
        const valuesInput = document.getElementById("grid-view-selected-rows" + componentName) as HTMLInputElement;
        const values = valuesInput.value.toString();
        let valuesList: string[] = [];

        if (obj.id === "jjcheckbox-select-all-rows") {
            return;
        }

        if (values.length > 0) {
            valuesList = values.split(",");
        }

        if (obj.checked) {
            if (valuesList.indexOf(obj.value) < 0) {
                valuesList.push(obj.value);
            }
        } else {
            valuesList = valuesList.filter((item) => item !== obj.value);
        }

        valuesInput.value = valuesList.join(",");

        let textInfo = "";
        const selectedText = document.getElementById("selected-text-" + componentName);
        if (valuesList.length === 0) {
            textInfo = selectedText?.getAttribute("no-record-selected-label") || "";
        } else if (valuesList.length === 1) {
            textInfo = selectedText?.getAttribute("one-record-selected-label") || "";
        } else {
            const multipleRecordsLabel = selectedText?.getAttribute("multiple-records-selected-label") || "";
            textInfo = multipleRecordsLabel.replace("{0}", valuesList.length.toString());
        }

        if (selectedText) {
            selectedText.textContent = textInfo;
        }
    }

    static selectAll(componentName) {
        const urlBuilder = new UrlBuilder()
        urlBuilder.addQueryParameter("context","selectAll")

        postFormValues({
            url: urlBuilder.build(),
            success: (data)=>{
                GridViewHelper.selectAllRowsElements(componentName, data.selectedRows)
            }
        })
    }
    
    static unSelectAll(componentName: string) {
        const checkboxes = document.querySelectorAll(`#${componentName} .jjselect input:not(:disabled)`) as NodeListOf<HTMLInputElement>;
        const valuesInput = document.getElementById("grid-view-selected-rows" + componentName) as HTMLInputElement;
        const selectedText = document.getElementById("selected-text-" + componentName);

        if (checkboxes) {
            checkboxes.forEach((checkbox) => {
                checkbox.checked = false;
            });
        }

        if (valuesInput) {
            valuesInput.value = "";
        }

        if (selectedText) {
            selectedText.textContent = selectedText.getAttribute("no-record-selected-label") || "";
        }
    }

}