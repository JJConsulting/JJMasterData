class JJUpload{
    static setup(){
        $("div.fileUpload").each(function () {
            let obj = $(this);
            let objid = obj.attr("id");
            let multiple = (obj.attr("jjmultiple") === "true");
            let autoSubmit = (obj.attr("autoSubmit") === "true");
            let maxFileSize = obj.attr("maxFileSize");
            let dragDrop = obj.attr("dragDrop");
            let copyPaste = obj.attr("copyPaste");
            let showFileSize = obj.attr("showFileSize");
            let allowedTypes = obj.attr("allowedTypes");
            let dragDropStr = "<span>&nbsp;<b>" + obj.attr("dragDropStr") + "</b></span>";
            let acceptFiles = "*";

            if (allowedTypes != "*") {
                acceptFiles = "." + allowedTypes.replace(" ", "").replace(",", ",.");
            }

            let frm = $("form");
            let surl = frm.attr("action");
            if (surl.includes("?"))
                surl += "&t=jjupload";
            else
                surl += "?t=jjupload";

            surl += "&objname=" + objid;

            // @ts-ignore
            let upload = $("#" + objid).uploadFile({
                url: surl,
                formData: frm.serializeArray(),
                fileName: "file",
                multiple: multiple,
                maxFileSize: maxFileSize, //bytes
                maxFileCount: 1000,
                dragDrop: dragDrop,
                showFileSize: showFileSize,
                dragdropWidth: ($(this).width() - 10),
                statusBarWidth: ($(this).width() - 10),
                autoSubmit: true,
                uploadButtonClass: "btn btn-primary",
                allowedTypes: allowedTypes,
                acceptFiles: acceptFiles,
                uploadStr: obj.attr("uploadStr"),
                dragDropStr: dragDropStr,
                doneStr: obj.attr("doneStr"),
                cancelStr: obj.attr("cancelStr"),
                abortStr: obj.attr("abortStr"),
                extErrorStr: obj.attr("extErrorStr"),
                sizeErrorStr: obj.attr("sizeErrorStr"),
                customErrorKeyStr: "jquery-upload-file-error",
                returnType: "json",
                onSubmit: function () {
                    showWaitOnPost = false;
                },
                onSuccess: function (files, data, xhr, pd) {
                    //files: list of files
                    //data: response from server
                    //xhr : jquery xhr object
                    var message = data["jquery-upload-file-message"];
                    if (message && message != "") {
                        var div = $("<div class='ajax-file-upload-filename'></div>");
                        $("<span class='fa fa-check-circle text-success'></span>").appendTo(div);
                        $("<span>&nbsp;" + message + "</span>").appendTo(div);
                        div.appendTo(pd.statusbar[0]);
                    }
                },
                afterUploadAll: function (obj) {
                    if (autoSubmit && obj.selectedFiles > 0) {
                        $("#uploadaction_" + objid).val("afteruploadall");
                        $("form:first").submit();
                    }
                }
            });

            $(window).resize(function () {
                $("#" + objid + " .ajax-upload-dragdrop").width($("#" + objid).width() - 30 + "px");
            });

            if (copyPaste == "true") {
                window.addEventListener('paste', e => {
                    // @ts-ignore
                    var files = e.clipboardData.files;
                    if (files.length == 1) {
                        var file = files[0];
                        if (file.type.indexOf("image") != -1) {
                            $("#btnDoUpload_" + objid).click(function () {
                                $("#preview_modal_" + objid).modal("hide");

                                var filename = $("#preview_filename_" + objid).val() as string;
                                if (filename.trim() == "") {
                                    filename = "image";
                                }
                                filename += ".png";

                                const dt = new DataTransfer();
                                const myNewFile = new File([file], filename, { type: file.type });
                                dt.items.add(myNewFile);

                                ($("#" + objid + " input[type='file']")[0] as HTMLInputElement).files = dt.files;
                                $("#" + objid + " input[type='file']").trigger("change");
                                return;
                            });

                            var reader = new FileReader();
                            reader.onload = function (event) {

                                (document.getElementById("pastedimage_" + objid) as HTMLImageElement).src = event.target.result.toString();

                                var filename = file.name.replace(/\.[^/.]+$/, "");


                                $("#preview_filename_" + objid).val(filename);
                                $("#preview_modal_" + objid).modal();
                            };
                            reader.readAsDataURL(file);
                            return;
                        }
                    }
                    
                    let selector = "#" + objid + " input[type='file']";
                    
                    ($()[0] as HTMLInputElement).files = files;
                    $("#" + objid + " input[type='file']").trigger("change");
                });
            }
        });
    }
}