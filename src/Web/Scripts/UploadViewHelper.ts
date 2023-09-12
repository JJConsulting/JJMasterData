class UploadViewHelper {
    static performFileAction(componentName: string, filename: string, action: string, promptMessage: string = null ) {
        const uploadActionInput = document.getElementById("upload-view-action-" + componentName) as HTMLInputElement;
        const filenameInput = document.getElementById("upload-view-file-name-" + componentName) as HTMLInputElement;
        
        if (uploadActionInput && filenameInput) {
            uploadActionInput.value = action;
            filenameInput.value = action === "renameFile" ? filename + ";" + prompt(promptMessage, filename) : filename;
        }
    }

    static clearFileAction(componentName: string, fileName: string){
        const uploadActionInput = document.getElementById("upload-view-action-" + componentName) as HTMLInputElement;
        const filenameInput = document.getElementById("upload-view-file-name-" + componentName) as HTMLInputElement;

        if (uploadActionInput && filenameInput) {
            uploadActionInput.value = String();
            filenameInput.value = String();
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
        eval(jsCallback);
        this.clearFileAction(componentName,fileName);
    }

    static downloadFile(componentName: string, fileName: string, jsCallback: string) {
        this.performFileAction(componentName, fileName, "downloadFile");
        eval(jsCallback)
        this.clearFileAction(componentName,fileName);
    }

    static renameFile(componentName: string, fileName: string, promptMessage: string, jsCallback: string) {
        this.performFileAction(componentName, fileName, "renameFile", promptMessage);
        eval(jsCallback)
        this.clearFileAction(componentName,fileName);
    }

}