class UploadAreaOptions {
    public componentName: string;
    public url: string;
    public allowMultipleFiles: boolean;
    public maxFileSize: number;
    public allowDragDrop: boolean;
    public showFileSize: boolean;
    public allowedTypes: string;
    public dragDropLabel: string;
    public jsCallback: string;
    public allowCopyPaste: boolean;
    public extensionNotAllowedLabel: string;
    public fileSizeErrorLabel: string;
    public abortLabel: string;
    public parallelUploads: number;
    public maxFiles: number;
    constructor(element: Element) {
        let dropzone =  element.querySelector<HTMLElement>(".dropzone") ;
        this.componentName = dropzone.getAttribute("id");
        this.allowMultipleFiles = element.getAttribute("allow-multiple-files") === "true";
        this.jsCallback = element.getAttribute("js-callback");
        this.allowCopyPaste = element.getAttribute("allow-copy-paste") === "true";
        this.maxFileSize = Number(element.getAttribute("max-file-size"));
        this.allowDragDrop = element.getAttribute("allow-drag-drop") === "true";
        this.showFileSize =element.getAttribute("show-file-size") === "true";
        this.allowedTypes = element.getAttribute("allowed-types");
        this.fileSizeErrorLabel = element.getAttribute("file-size-error-label");
        this.dragDropLabel = element.getAttribute("drag-drop-label");
        this.abortLabel = element.getAttribute("abort-label")
        this.maxFiles = Number(element.getAttribute("max-files"))
        this.parallelUploads = Number(element.getAttribute("parallel-uploads"))
        this.extensionNotAllowedLabel = element.getAttribute("extension-not-allowed-label");
        this.url = element.getAttribute("upload-url");
        
        if(!this.url){
            let routeContext = element.getAttribute("route-context");
            let queryStringParams = element.getAttribute("query-string-params");
            let urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("routeContext", routeContext)

            const params = queryStringParams.split('&');

            for (let i = 0; i < params.length; i++) {
                const param = params[i].split('=');
                const key = decodeURIComponent(param[0]);
                const value = decodeURIComponent(param[1]);
                urlBuilder.addQueryParameter(key, value);
            }
            this.url = urlBuilder.build();
        }
    }
}