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
    onModalHidden?: Function;

    public constructor() {
        this.modalId = "jjmasterdata-modal";
        this.modalSize = ModalSize.ExtraLarge;

    }
    abstract showIframe(url: string, title: string, size: ModalSize);

    abstract showUrl(options: ModalUrlOptions, title: string, size: ModalSize) : Promise<any>;
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
    
    private getBootstrapModal(){
        return bootstrap.Modal.getOrCreateInstance("#" + this.modalId);
    }
    
    private showModal(){
        this.getBootstrapModal().show();
    }

    private hideModal(){
        this.getBootstrapModal().hide();
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
            
            const onModalHidden = this.onModalHidden;
            
            this.modalElement.addEventListener('hidden.bs.modal', () => {
                if(onModalHidden){
                    onModalHidden();
                }
            })

        } else {
            this.modalElement = document.getElementById(this.modalId);
            
            const dialog = document.getElementById(this.modalId + "-dialog");

            //@ts-ignore
            Object.values(ModalSize).forEach(cssClass => {
                //@ts-ignore
                dialog.classList.remove(cssClass)
            });

            dialog.classList.add(this.getModalCssClass())
        }
    }

    override showIframe(url: string, title: string, size: ModalSize = null) {
        this.modalTitle = title;
        this.modalSize = size || this.modalSize;
        this.createModalElement();
        const modalBody = this.modalElement.querySelector(".modal-body");
        
        let style = "width: 100vw; height: 100vh;";
        
        modalBody.innerHTML = `<iframe src="${url}" frameborder="0" style="${style}"></iframe>`;

        this.showModal();
    }

    override async showUrl(options: ModalUrlOptions, title: string, size: ModalSize = null) {
        this.modalTitle = title;
        this.modalSize = size ?? ModalSize.Default;
        this.createModalElement();

        return await fetch(options.url, options.requestOptions)
            .then( async response => {
                if (response.headers.get("content-type")?.includes("application/json")) {
                    return response.json();
                } 
                else if(response.redirected){
                    window.open(response.url, '_blank').focus();
                }
                else {
                   return response.text().then((htmlData)=>{
                        this.setAndShowModal(htmlData)
                    });
                }
        })
    }
    
    private setAndShowModal(content: string){
        const modalBody = this.modalElement.querySelector<HTMLElement>(`#${this.modalId} .modal-body`);
        this.setInnerHTML(modalBody, content)

        this.showModal();
    }


    private setInnerHTML(element: HTMLElement, html: string): void {
        element.innerHTML = html;

        Array.from(element.querySelectorAll("script")).forEach((oldScriptElement: HTMLScriptElement) => {
            const newScriptElement = document.createElement("script");

            Array.from(oldScriptElement.attributes).forEach((attr) => {
                newScriptElement.setAttribute(attr.name, attr.value);
            });

            const scriptText = document.createTextNode(oldScriptElement.innerHTML);
            newScriptElement.appendChild(scriptText);

            oldScriptElement.parentNode?.replaceChild(newScriptElement, oldScriptElement);
        });
    }

    hide() {
        this.hideModal();
    }
}

class _LegacyModal extends ModalBase {
    
    constructor() {
        super();
        let onModalHidden = this.onModalHidden;
        $("#"+this.modalId).on('hidden.bs.modal', function () {
            onModalHidden()
        });
    }
    
    private createModalHtml(content: string, isIframe: boolean) {
        const size = isIframe
            ? this.modalSize === ModalSize.Small
                ? "auto"
                : this.modalSize === ModalSize.ExtraLarge
                    ? "65%"
                    : "auto"
            : "auto";

        const html = `
            <div id="${this.modalId}" tabindex="-1" class="modal fade" role="dialog">
                <div class="modal-dialog" style="position: auto; height: ${
            this.modalSize === ModalSize.ExtraLarge
                ? "95"
                : this.modalSize === ModalSize.Large
                    ? "75"
                    : this.modalSize === ModalSize.Fullscreen
                        ? "100"
                        : "90"
        }vh; width: ${size};">
                    <div class="modal-content" style="height:100%;width:auto;">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal">&times;</button>
                            <h4 class="modal-title" id="${this.modalId}-title"></h4>
                        </div>
                        <div class="modal-body" style="height:90%;width:auto;">
                            ${isIframe ? `<iframe style="border: 0px;" src="${content}" width="100%" height="97%">Waiting...</iframe>` : content}
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
        const modalHtml = this.createModalHtml(url, true); // Using iframe
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
                const modalHtml = this.createModalHtml(content, false); // Not using iframe
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




class Modal {
    private instance: ModalBase;

    constructor() {
        if (bootstrapVersion === 5) {
            this.instance = new _Modal();
        } else {
            this.instance = new _LegacyModal();
        }

        this.instance.modalId = "jjmasterdata-modal";
        this.instance.modalSize = ModalSize.ExtraLarge;
    }

    showIframe(url: string, title: string, size: ModalSize = null) {
        this.instance.showIframe(url,title,size);
    }
    async showUrl(options: ModalUrlOptions, title: string, size: ModalSize = null): Promise<any> {
        return await this.instance.showUrl(options,title,size);
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

    get onModalHidden(): Function {
        return this.instance.onModalHidden;
    }

    set onModalHidden(value: Function) {
        this.instance.onModalHidden = value;
    }
}
var defaultModal = function () {
    if (!(this instanceof Modal)) {
        return new Modal();
    }
}();

// Compatibility with legacy systems.
class popup{
    //Yes, the parameters are inverted.
    static show(title,url,size = null){
        defaultModal.showIframe(url,title,size)
    }
    static hide(){
        defaultModal.hide()
    }
}
