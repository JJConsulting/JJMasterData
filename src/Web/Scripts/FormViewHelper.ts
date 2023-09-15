class FormViewHelper {
    static showInsertSuccess(componentName: string) {
        const insertAlertDiv = document.getElementById("insert-alert-div-item");
        
        setTimeout(function () {
            insertAlertDiv.style.opacity = "0";
        }, 1000); 
        
        setTimeout(function () {
            insertAlertDiv.style.display = "none";
        }, 3000); 
    }

    static openSelectElementInsert(componentName: string, encryptedActionMap: string) {
        const currentActionInput = document.querySelector(`#form-view-current-action-${componentName}`) as HTMLInputElement;
        const selectActionValuesInput = document.querySelector(`#form-view-select-action-values-${componentName}`) as HTMLInputElement;
        const form = document.querySelector('form') as HTMLFormElement;

        if (currentActionInput && selectActionValuesInput && form) {
            currentActionInput.value = 'ELEMENTSEL';
            selectActionValuesInput.value = encryptedActionMap;
            form.requestSubmit();
        }
    }
}
