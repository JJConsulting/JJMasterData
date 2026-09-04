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
        if (confirmationMessage) {
            const confirmed = confirm(confirmationMessage)
            if (!confirmed) {
                return
            }
        }

        this.performFileAction(componentName, fileName, "deleteFile");
        eval(jsCallback);
        this.clearFileAction(componentName, fileName);
    }

    static markDeleted(componentName: string, fileName: string, confirmationMessage: string, jsCallback: string, deletedInputId: string) {
        if (confirmationMessage) {
            const confirmed = confirm(confirmationMessage)
            if (!confirmed) {
                return
            }
        }

        const deletedInput = document.getElementById(deletedInputId) as HTMLInputElement;
        if (!deletedInput) {
            return;
        }

        const deletedFiles = deletedInput.value
            .split(",")
            .map(value => value.trim())
            .filter(value => value.length > 0);

        if (deletedFiles.indexOf(fileName) === -1) {
            deletedFiles.push(fileName);
            deletedInput.value = deletedFiles.join(",");
        }

        eval(jsCallback);
    }

    static downloadFile(componentName: string, fileName: string, jsCallback: string) {
        this.performFileAction(componentName, fileName, "downloadFile");
        eval(jsCallback)
        this.clearFileAction(componentName,fileName);
    }

    static renameFile(componentName: string, fileName: string, promptMessage: string, jsCallback: string) {
        const newName = prompt(promptMessage, fileName);
        if (newName === null || newName === fileName) {
            return;
        }

        const uploadActionInput = document.getElementById("upload-view-action-" + componentName) as HTMLInputElement;
        const filenameInput = document.getElementById("upload-view-file-name-" + componentName) as HTMLInputElement;
        if (uploadActionInput && filenameInput) {
            uploadActionInput.value = "renameFile";
            filenameInput.value = fileName + ";" + newName;
        }

        eval(jsCallback)
        this.clearFileAction(componentName,fileName);
    }

}
