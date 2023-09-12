class UploadViewHelper {
    static performFileAction(componentName: string, filename: string, action: string, promptMessage: string = null ) {
        const uploadActionInput = document.getElementById("upload-action-" + componentName) as HTMLInputElement;
        const filenameInput = document.getElementById("filename-" + componentName) as HTMLInputElement;
        
        if (uploadActionInput && filenameInput) {
            uploadActionInput.value = action;
            filenameInput.value = action === "renameFile" ? filename + ";" + prompt(promptMessage, filename) : filename;
        }
    }

    static deleteFile(componentName: string, fileName: string, confirmationMessage: string, jsCallback: string) {
        if(confirmationMessage){
            const confirmed = confirm(confirmationMessage)
            if(!confirmed){
                return
            }
        }
        
        this.performFileAction(componentName, fileName, "deleteFile");
        eval(jsCallback)
    }

    static downloadFile(componentName: string, fileName: string) {
        this.performFileAction(componentName, fileName, "downloadFile");
    }

    static renameFile(componentName: string, fileName: string, promptMessage: string, jsCallback: string) {
        this.performFileAction(componentName, fileName, "renameFile", promptMessage);
        eval(jsCallback)
    }

}