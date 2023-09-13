class FormViewHelper {
    static showInsertSuccess(componentName: string) {
        const insertMessagePanel = document.getElementById("insert-message-panel" + componentName) as HTMLElement;
        const insertPanel = document.getElementById("insert-panel" + componentName) as HTMLElement;

        if (insertMessagePanel && insertPanel) {
            insertMessagePanel.style.transition = "opacity 2s ease";
            insertMessagePanel.style.opacity = "0";

            setTimeout(() => {
                insertMessagePanel.style.display = "none";
                insertPanel.style.display = "block";
            }, 2000);
        }
    }

    static openSelectElementInsert(componentName: string, encryptedActionMap: string) {
        const currentActionInput = document.querySelector(`#form-view-current-action-${componentName}`) as HTMLInputElement;
        const selectActionValuesInput = document.querySelector(`#form-view-select-action-values${componentName}`) as HTMLInputElement;
        const form = document.querySelector('form') as HTMLFormElement;

        if (currentActionInput && selectActionValuesInput && form) {
            currentActionInput.value = 'ELEMENTSEL';
            selectActionValuesInput.value = encryptedActionMap;
            form.requestSubmit();
        }
    }
}
