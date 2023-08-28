class FileUploadOptions {
    private componentName: string;
    private url: string;
    private form: string;
    private allowMultiple: boolean;
    private maxFileSize: Number;
    private allowDragDrop: boolean;
    private showFileSize: boolean;
    private allowedTypes: string;
    private dragDropLabel: string;
    private autoSubmit: boolean;
    constructor(componentName, url, form, allowMultiple, maxFileSize, allowDragDrop, showFileSize, allowedTypes, dragDropLabel, autoSubmit) {
        this.componentName = componentName;
        this.url = url;
        this.form = form;
        this.allowMultiple = allowMultiple;
        this.maxFileSize = maxFileSize;
        this.allowDragDrop = allowDragDrop;
        this.showFileSize = showFileSize;
        this.allowedTypes = allowedTypes;
        this.dragDropLabel = dragDropLabel;
        this.autoSubmit = autoSubmit;
    }
}

class UploadArea {
    static uploadFile(options) {
        
        const selector = "#" + options.componentName;
        // @ts-ignore
        $(selector).uploadFile({
            url: options.url,
            formData: $(options.form).serializeArray(),
            fileName: "file",
            multiple: options.allowMultiple,
            maxFileSize: options.maxFileSize,
            maxFileCount: 1000,
            dragDrop: options.allowDragDrop,
            showFileSize: options.showFileSize,
            dragdropWidth: ($(selector).width() - 10),
            statusBarWidth: ($(selector).width() - 10),
            autoSubmit: true,
            uploadButtonClass: "btn btn-primary",
            allowedTypes: options.allowedTypes,
            acceptFiles: options.allowedTypes !== "*" ? "." + options.allowedTypes.replace(" ", "").replace(",", ",.") : "*",
            uploadStr: $(selector).attr("uploadStr"),
            dragDropStr: "<span>&nbsp;<b>" + options.dragDropLabel + "</b></span>",
            doneStr: $(selector).attr("doneStr"),
            cancelStr: $(selector).attr("cancelStr"),
            abortStr: $(selector).attr("abortStr"),
            extErrorStr: $(selector).attr("extErrorStr"),
            sizeErrorStr: $(selector).attr("sizeErrorStr"),
            customErrorKeyStr: "jquery-upload-file-error",
            returnType: "json",
            onSubmit: function () {
                showWaitOnPost = false;
            },
            onSuccess: function (files, data, xhr, pd) {
                var message = data["jquery-upload-file-message"];
                if (message && message != "") {
                    var div = $("<div class='ajax-file-upload-filename'></div>");
                    $("<span class='fa fa-check-circle text-success'></span>").appendTo(div);
                    $("<span>&nbsp;" + message + "</span>").appendTo(div);
                    div.appendTo(pd.statusbar[0]);
                }
            },
            afterUploadAll: function (element) {
                if (options.autoSubmit && element.selectedFiles > 0) {
                    $("#upload-action-" + options.componentName).val("afteruploadall");
                }
                loadJJMasterData()
            },
        });
    }

    private static handleCopyPaste(componentName: string) {
        window.addEventListener("paste", (e) => {
            var files = e.clipboardData.files;
            if (files.length === 1) {
                var file = files[0];
                if (file.type.indexOf("image") !== -1) {
                    document.querySelector("#btnDoUpload_" + componentName).addEventListener("click", () => {
                        document.querySelector("#preview_modal_" + componentName).classList.add("hide");

                        var filename = document.querySelector<HTMLInputElement>("#preview_filename-" + componentName).value || "image";
                        filename += ".png";

                        const dt = new DataTransfer();
                        const myNewFile = new File([file], filename, { type: file.type });
                        dt.items.add(myNewFile);

                        document.querySelector<HTMLInputElement>("#" + componentName + " input[type='file']").files = dt.files;
                        document.querySelector("#" + componentName + " input[type='file']").dispatchEvent(new Event("change"));
                        return;
                    });

                    var reader = new FileReader();
                    reader.onload = function (event) {
                        document.querySelector<HTMLImageElement>("#pastedimage_" + componentName).src = event.target.result.toString();

                        var filename = file.name.replace(/\.[^/.]+$/, "");
                        document.querySelector<HTMLInputElement>("#preview_filename-" + componentName).value = filename;
                        document.querySelector("#preview_modal_" + componentName).classList.remove("hide");
                    };
                    reader.readAsDataURL(file);
                    return;
                }
            }

            document.querySelector<HTMLInputElement>("#" + componentName + " input[type='file']").files = files;
            document.querySelector("#" + componentName + " input[type='file']").dispatchEvent(new Event("change"));
        });
    }

    static setup() {
        document.querySelectorAll("div.fileUpload").forEach((element) => {
            let componentName = element.getAttribute("id");
            let multiple = element.getAttribute("jjmultiple") === "true";
            let autoSubmit = element.getAttribute("autoSubmit") === "true";
            let maxFileSize = element.getAttribute("maxFileSize");
            let dragDrop = element.getAttribute("dragDrop");
            let copyPaste = element.getAttribute("copyPaste");
            let showFileSize = element.getAttribute("showFileSize");
            let allowedTypes = element.getAttribute("allowedTypes");
            let dragDropStr = "<span>&nbsp;<b>" + element.getAttribute("dragDropStr") + "</b></span>";

            let frm = document.querySelector("form");
            
            let url : string;
            
            if(element.getAttribute("url") != null){
                url = element.getAttribute("url");
            }
            else{
                let urlBuilder = new UrlBuilder();
                urlBuilder.addQueryParameter("context","fileUpload")
                urlBuilder.addQueryParameter("componentName",componentName)
                url = urlBuilder.build();
            }

            const fileUploadOptions = new FileUploadOptions(
                componentName,
                url,
                frm,
                multiple,
                maxFileSize,
                dragDrop,
                showFileSize,
                allowedTypes,
                dragDropStr,
                autoSubmit
            );

            this.uploadFile(fileUploadOptions);

            window.addEventListener("resize", () => {
                document.querySelector<HTMLElement>("#" + componentName + " .ajax-upload-dragdrop").style.width =
                    document.querySelector("#" + componentName).clientWidth - 30 + "px";
            });

            if (copyPaste === "true") {
                this.handleCopyPaste(componentName)
            }
        });
    }
}