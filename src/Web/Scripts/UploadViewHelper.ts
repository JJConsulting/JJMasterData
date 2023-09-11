class UploadViewHelper {
    static show(componentName: string, title:string, values: string){

        const panelName = $("#v_" + componentName).attr("panelName");
        

        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("uploadView-" + panelName, componentName)
        urlBuilder.addQueryParameter("uploadViewParams",values)
        const url = urlBuilder.build();
        
        
        const modalId = componentName + "-upload-modal";

        const modal = new Modal();
        modal.modalId =modalId;
        
        modal.showUrl({url:url},title, ModalSize.Fullscreen).then(_=>{
            listenAllEvents("#" + modalId)
        })
    }

    static performFileAction(componentName, filename, action, promptStr = null) {
        if (promptStr && !confirm(promptStr)) {
            return false;
        }

        const uploadActionInput = document.getElementById("upload-action-" + componentName) as HTMLInputElement;
        const filenameInput = document.getElementById("filename-" + componentName) as HTMLInputElement;
        const form = document.querySelector("form") as HTMLFormElement;

        if (uploadActionInput && filenameInput && form) {
            uploadActionInput.value = action;
            filenameInput.value = action === "RENAMEFILE" ? filename + ";" + prompt(promptStr, filename) : filename;
            form.dispatchEvent(new Event("submit", { bubbles: true, cancelable: true }));
            if (action === "DOWNLOADFILE") {
                setTimeout(() => {
                    SpinnerOverlay.hide();
                    uploadActionInput.value = "";
                }, 1500);
            }
        }
        return true;
    }

    static deleteFile(componentName, filename, promptStr) {
        return this.performFileAction(componentName, filename, "DELFILE", promptStr);
    }

    static downloadFile(componentName, filename) {
        this.performFileAction(componentName, filename, "DOWNLOADFILE");
    }

    static renameFile(componentName, filename, promptStr) {
        this.performFileAction(componentName, filename, "RENAMEFILE", promptStr);
    }

}