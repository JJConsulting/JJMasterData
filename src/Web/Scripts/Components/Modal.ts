enum ModalSize {
    Default,
    ExtraLarge,
    Large,
    Small,
    Fullscreen
}

class ModalUrlOptions{
    url: string
    requestOptions?: RequestInit
}

abstract class ModalBase{
    modalId: string;
    modalTitle: string;
    modalSize: ModalSize;
    modalElement: HTMLElement;
    centered: boolean;

    public constructor() {
        this.modalId = "jjmasterdata-modal";
        this.modalSize = ModalSize.Default;
    }

    abstract showIframe(url: string, title: string, size: ModalSize);

    abstract showUrl(options: ModalUrlOptions, title: string, size: ModalSize) : Promise<void>;
    abstract hide();
}

class _Modal extends ModalBase {

    private modalSizeCssClass = {
        Default: "jj-modal-default",
        ExtraLarge: "jj-modal-xl",
        Large: "jj-modal-lg",
        Small: "jj-modal-sm",
        Fullscreen: "modal-fullscreen",
    };
    
    private showModal(){
        if(bootstrapVersion >= 5){
            const bootstrapModal = new bootstrap.Modal(this.modalElement);
            bootstrapModal.show();
        }
        else{
            //@ts-ignore
            $("#" + this.modalId).modal("show");
        }
    }

    private hideModal(){
        if(bootstrapVersion >= 5){
            const bootstrapModal = new bootstrap.Modal(this.modalElement);
            bootstrapModal.hide();
        }
        else{
            //@ts-ignore
            $("#" + this.modalId).modal("hide");
        }
    }
    
    private getModalCssClass(){
        return this.modalSizeCssClass[ModalSize[this.modalSize]];
    }
    
    private createModalElement() {
        if (!document.getElementById(this.modalId)) {
            this.modalElement = document.createElement("div");
            this.modalElement.id = this.modalId;
            this.modalElement.classList.add("modal", "fade");
            this.modalElement.tabIndex = -1;
            this.modalElement.setAttribute("role", "dialog");
            this.modalElement.setAttribute("aria-labelledby", `${this.modalId}-label`);
            this.modalElement.innerHTML = `
      <div id="${this.modalId}-dialog" class="modal-dialog ${this.centered ? "modal-dialog-centered" : ""} modal-dialog-scrollable ${this.getModalCssClass()}" role="document">
        <div class="modal-content" >
          <div class="modal-header">
            <h5 class="modal-title" id="${this.modalId}-label">${this.modalTitle}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body"> </div>
        </div>
      </div>`;
            let form = document.forms[0];
            if(form){
                form.appendChild(this.modalElement);
            }
            else{
                document.body.appendChild(this.modalElement);
            }
        } else {
            this.modalElement = document.getElementById(this.modalId);

            const dialog = document.getElementById(this.modalId + "-dialog");

            //@ts-ignore
            Object.values(ModalSize).forEach(cssClass => {
                dialog.classList.remove(cssClass)
            });
            
            dialog.classList.add(this.getModalCssClass())
        }
    }

    override showIframe(url: string, title: string, size: ModalSize = null) {
        this.modalTitle = title;
        this.modalSize = size ?? ModalSize.Default;
        this.createModalElement();
        const modalBody = this.modalElement.querySelector(".modal-body");
        
        let style = bootstrapVersion == 5 ?"width: 100vw; height: 100vh;" : "width: 100%; height: 100%;";
        
        modalBody.innerHTML = `<iframe src="${url}" frameborder="0" style="${style}"></iframe>`;

        this.showModal();
    }

    override async showUrl(options: ModalUrlOptions, title: string, size: ModalSize = null) {
        this.modalTitle = title;
        this.modalSize = size ?? ModalSize.Default;
        this.createModalElement();
        const modalBody = this.modalElement.querySelector(".modal-body");
        await fetch(options.url, options.requestOptions)
            .then((response) => response.text())
            .then((content) => {
                modalBody.innerHTML = content;

                this.showModal();
            });
    }

    hide() {
        this.hideModal();
    }
}

class _LegacyModal extends ModalBase {
    private createModalHtml(url: string) {
        const iframeSize =
            this.modalSize === ModalSize.Small
                ? "<div class=\"modal-dialog\" style=\"margin:0.7em;left:0px;right:0px;top:0px;bottom:0px; position:fixed;width:auto;\">\r\n"
                : `<div class="modal-dialog" style="position: auto; height: ${
                    this.modalSize === ModalSize.ExtraLarge
                        ? "95"
                        : this.modalSize === ModalSize.Large
                            ? "75"
                            : this.modalSize === ModalSize.Fullscreen
                                ? "100"
                                : "90"
                }vh; width: ${
                    this.modalSize === ModalSize.ExtraLarge ? "65%" : "auto"
                };">\r\n`;

        const html = `
            <div id="${this.modalId}" tabindex="-1" class="modal fade" role="dialog">
                ${iframeSize}
                    <div class="modal-content" style="height:100%;width:auto;">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal">&times;</button>
                            <h4 class="modal-title" id="${this.modalId}-title"></h4>
                        </div>
                        <div class="modal-body" style="height:90%;width:auto;">
                            <iframe style="border: 0px;" src="${url}" width="100%" height="97%">Waiting...</iframe>
                        </div>
                    </div>
                </div>
            </div>
        `;

        return html;
    }

    private showModal() {
        $(`#${this.modalId}`).modal();

        $("iframe").on("load", () => {
            SpinnerOverlay.hide();
        });
    }

    private setTitle(title: string) {
        $(`#${this.modalId}-title`).html(title);
    }

    override showIframe(url: string, title: string, size: ModalSize = null) {
        this.modalSize = size || this.modalSize;
        const modalHtml = this.createModalHtml(url);
        $(modalHtml).appendTo($("body"));
        this.setTitle(title);
        this.showModal();
    }

    override async showUrl(options: ModalUrlOptions, title: string, size: ModalSize = null) {
        this.modalSize = size || this.modalSize;

        try {
            const response = await fetch(options.url, options.requestOptions);
            if (response.ok) {
                const content = await response.text();
                const modalHtml = this.createModalHtml(`data:text/html,${encodeURIComponent(content)}`);
                $(modalHtml).appendTo($("body"));
                this.setTitle(title);
                this.showModal();
            } else {
                console.error(`Failed to fetch content from URL: ${options.url}`);
            }
        } catch (error) {
            console.error("An error occurred while fetching content:", error);
        }
    }

    override hide() {
        $(`#${this.modalId}`).modal("hide");
    }
}

var defaultModal = function () {
    if(bootstrapVersion == 5){
        if (!(this instanceof _Modal)) {
            return new _Modal();
        }
    }
    else{
        if (!(this instanceof _LegacyModal)) {
            return new _LegacyModal();
        }
    }
}();


class Modal {
    private instance: ModalBase;
    constructor() {
        if (bootstrapVersion === 5) {
            this.instance = new _Modal();
        } else {
            this.instance = new _LegacyModal();
        }

        this.instance.modalId = "jjmasterdata-modal";
        this.instance.modalSize = ModalSize.Default;
    }

    showIframe(url: string, title: string, size: ModalSize) {
        this.instance.showIframe(url,title,size);
    }
    async showUrl(options: ModalUrlOptions, title: string, size: ModalSize = null): Promise<void> {
        await this.instance.showUrl(options,title,size);
    }
    hide() {
        this.instance.hide();
    }
    
    get modalId(): string {
        return this.instance.modalId;
    }

    set modalId(value: string) {
        this.instance.modalId = value;
    }

    get modalTitle(): string {
        return this.instance.modalTitle;
    }

    set modalTitle(value: string) {
        this.instance.modalTitle = value;
    }

    get modalSize(): ModalSize {
        return this.instance.modalSize;
    }

    set modalSize(value: ModalSize) {
        this.instance.modalSize = value;
    }

    get modalElement(): HTMLElement {
        return this.instance.modalElement;
    }

    set modalElement(value: HTMLElement) {
        this.instance.modalElement = value;
    }

    get centered(): boolean {
        return this.instance.centered;
    }

    set centered(value: boolean) {
        this.instance.centered = value;
    }
}

