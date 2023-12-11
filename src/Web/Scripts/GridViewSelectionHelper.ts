class GridViewSelectionHelper{
    static selectItem(componentName: string, inputElement: HTMLInputElement) {
        const valuesInput = document.getElementById("grid-view-selected-rows-" + componentName) as HTMLInputElement;
        const values = valuesInput.value.toString();
        let valuesList: string[] = [];

        if (inputElement.id === `${componentName}-checkbox-select-all-rows`) {
            return;
        }

        if (values.length > 0) {
            valuesList = values.split(",");
        }

        if (inputElement.checked) {
            if (valuesList.indexOf(inputElement.value) < 0) {
                valuesList.push(inputElement.value);
            }
        } else {
            valuesList = valuesList.filter((item) => item !== inputElement.value);
        }

        valuesInput.value = valuesList.join(",");

        let textInfo: string;
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
    
    static selectAllAtSamePage(componentName: string){
        const checkboxes = document.querySelectorAll(`#${componentName} td.jj-checkbox input`);

        const selectAllCheckbox = document.querySelector<HTMLInputElement>(`#${componentName}-checkbox-select-all-rows`);
        const isSelectAllChecked = selectAllCheckbox.checked;
        
        checkboxes.forEach(function(checkbox: HTMLInputElement) {
            if (!checkbox.disabled) {
                checkbox.checked = isSelectAllChecked;
                const event = new Event('change');
                checkbox.dispatchEvent(event);
            }
        });

    }

    static selectAll(componentName, routeContext) {
        const urlBuilder = new UrlBuilder()
        urlBuilder.addQueryParameter("routeContext",routeContext)
        postFormValues({
            url: urlBuilder.build(),
            success: (data)=>{
                this.selectAllRowsElements(componentName, data.selectedRows)
            }
        })
    }

    static selectAllRowsElements(componentName, rows) {
        const values = rows.split(",");

        const checkboxes = document.querySelectorAll<HTMLInputElement>(`#grid-view-table-${componentName} .jj-checkbox input:not(:disabled)`);
        checkboxes.forEach(checkbox => checkbox.checked = true);

        const selectedRowsInput = document.getElementById("grid-view-selected-rows-" + componentName) as HTMLInputElement;
        selectedRowsInput.value = values.join(",");

        const selectedText = document.getElementById("selected-text-" + componentName);
        selectedText.textContent = selectedText.getAttribute("multiple-records-selected-label").replace("{0}", values.length.toString());
    }


    static unSelectAll(componentName: string) {
        const checkboxes = document.querySelectorAll(`#grid-view-table-${componentName} .jj-checkbox input:not(:disabled)`) as NodeListOf<HTMLInputElement>;
        const valuesInput = document.getElementById("grid-view-selected-rows-" + componentName) as HTMLInputElement;
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