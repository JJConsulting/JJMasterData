class UploadAreaOptions {
    public componentName: string;
    public url: string;
    public allowMultipleFiles: boolean;
    public maxFileSize: Number;
    public allowDragDrop: boolean;
    public showFileSize: boolean;
    public allowedTypes: string;
    public dragDropLabel: string;
    public jsCallback: string;
    public allowCopyPaste: boolean;
    public extensionNotAllowedLabel: string;
    public fileSizeErrorLabel: string;
    public abortLabel: string;
    public parallelUploads: Number;

    constructor(element: Element) {
        let dropzone = element.lastChild as Element;
        this.componentName = dropzone.getAttribute("id");
        this.allowMultipleFiles = element.getAttribute("allow-multiple-files") === "true";
        this.jsCallback = element.getAttribute("js-callback");
        this.allowCopyPaste = Boolean(element.getAttribute("allow-copy-paste"));
        this.maxFileSize = Number(element.getAttribute("max-file-size"));
        this.allowDragDrop = Boolean(element.getAttribute("allow-drag-drop"));
        this.showFileSize = Boolean(element.getAttribute("show-file-size"));
        this.allowedTypes = element.getAttribute("allowed-types");
        this.fileSizeErrorLabel = element.getAttribute("file-size-error-label");
        this.dragDropLabel = element.getAttribute("drag-drop-label");
        this.abortLabel = element.getAttribute("abort-label")
        this.parallelUploads = Number(element.getAttribute("parallel-uploads"))
        this.extensionNotAllowedLabel = element.getAttribute("extension-not-allowed-label");

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