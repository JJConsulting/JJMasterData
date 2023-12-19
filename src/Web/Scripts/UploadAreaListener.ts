class UploadAreaListener {
    static configureFileUpload(options: UploadAreaOptions) {
        
        const selector = "div#" + options.componentName;
        
        let dropzone = new window.Dropzone(selector, {
            paramName: "uploadAreaFile",
            maxFilesize: options.maxFileSize,
            uploadMultiple: options.allowMultipleFiles,
            method: "POST",
            acceptedFiles: options.allowedTypes,
            maxFiles: options.maxFiles,
            dictDefaultMessage :options.dragDropLabel,
            dictFileTooBig: options.fileSizeErrorLabel,
            dictUploadCanceled: options.abortLabel,
            dictInvalidFileType: options.extensionNotAllowedLabel,
            clickable:true,
            parallelUploads: options.parallelUploads,
            url: options.url
        });

        const onSuccess = (files = null) => {
            const processFile = (file: Dropzone.DropzoneFile) => {
                const jsonResponse = JSON.parse(file.xhr.responseText);
                if (jsonResponse.error) {
                    const previewElement = file.previewElement;
                    previewElement.classList.remove("dz-success");
                    previewElement.classList.add("dz-error");
                    const errorElement = previewElement.querySelector('.dz-error-message');
                    errorElement.textContent = jsonResponse.error;
                    return;
                }

                if (dropzone.getQueuedFiles().length === 0) {
                    if (options.jsCallback) {
                        eval(options.jsCallback);
                    }
                }
            };

            if (Array.isArray(files)) {
                files.forEach(processFile);
            } else {
                processFile(files);
            }
        };
        
        if(options.allowMultipleFiles){
            dropzone.on("successmultiple",onSuccess)
        }
        else{
            dropzone.on("success",onSuccess)
        }

        if (options.allowCopyPaste) {
            document.onpaste = function (event) {
                const items = Array.from(event.clipboardData.files);
                items.forEach((item) => {
                    //@ts-ignore
                    dropzone.addFile(item);
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