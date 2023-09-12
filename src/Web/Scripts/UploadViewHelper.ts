class UploadViewHelper {
    static performFileAction(componentName, filename, action, promptMessage = null) {
        const uploadActionInput = document.getElementById("upload-action-" + componentName) as HTMLInputElement;
        const filenameInput = document.getElementById("filename-" + componentName) as HTMLInputElement;
        const form = document.querySelector("form") as HTMLFormElement;

        if (uploadActionInput && filenameInput && form) {
            uploadActionInput.value = action;
            filenameInput.value = action === "RENAMEFILE" ? filename + ";" + prompt(promptMessage, filename) : filename;
            if (action === "DOWNLOADFILE") {
                setTimeout(() => {
                    SpinnerOverlay.hide();
                    uploadActionInput.value = "";
                }, 1500);
            }
        }
    }

    static deleteFile(componentName, filename, confirmationMessage) {
        if(confirmationMessage){
            const confirmed = confirm(confirmationMessage)
            if(confirmed){
                return
            }
        }
        
        this.performFileAction(componentName, filename, "DELFILE");
    }

    static downloadFile(componentName, filename) {
        this.performFileAction(componentName, filename, "DOWNLOADFILE");
    }

    static renameFile(componentName, filename, promptMessage) {
        this.performFileAction(componentName, filename, "RENAMEFILE", promptMessage);
    }

}