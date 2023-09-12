
class UploadAreaListener {
    static configureFileUpload(options: UploadAreaOptions) {
        
        const selector = "div#" + options.componentName;
        
        let dropzone = new window.Dropzone(selector, {
            paramName: "uploadAreaFile",
            maxFilesize: options.maxFileSize,
            uploadMultiple: options.allowMultipleFiles,
            method: "POST",
            dictDefaultMessage :options.dragDropLabel,
            dictFileTooBig: options.fileSizeErrorLabel,
            dictUploadCanceled: options.abortLabel,
            dictInvalidFileType: options.extensionNotAllowedLabel,
            clickable:true,
            parallelUploads: options.parallelUploads,
            url: options.url
        });
        
        const onSuccess = (file = null)=>{
            if(dropzone.getQueuedFiles().length === 0){
                const areFilesUploadedInput = document.querySelector<HTMLInputElement>("#" + options.componentName + "-are-files-uploaded");

                if(areFilesUploadedInput){
                    areFilesUploadedInput.value = "1";
                }
                
                if(options.jsCallback){
                    eval(options.jsCallback)
                }
            }
        }

        dropzone.on("success",onSuccess)
        dropzone.on("successmultiple",onSuccess)

        if (options.allowCopyPaste) {
            document.onpaste = function (event) {
                const items = Array.from(event.clipboardData.items);
                items.forEach((item) => {
                    if (item.kind === 'file') {
                        //@ts-ignore
                        dropzone.addFile(item.getAsFile());
                    }
                });
            };
        }
    }

    static listenFileUpload(selectorPrefix = String()){
        document.querySelectorAll(selectorPrefix + "div.upload-area-div").forEach((element) => {
            const uploadAreaOptions = new UploadAreaOptions(element)
            this.configureFileUpload(uploadAreaOptions);
        });
    }
}