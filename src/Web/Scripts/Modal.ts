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

class Modal {
    modalId: string;
    modalTitle: string;
    modalSize: ModalSize;
    modalElement: HTMLElement;
    centered: boolean;

    private modalSizeCssClass = {
        Default: "jj-modal-default",
        ExtraLarge: "jj-modal-xl",
        Large: "jj-modal-lg",
        Small: "jj-modal-sm",
        Fullscreen: "modal-fullscreen",
    };

    constructor() {
        this.modalId = "jjmasterdata-modal";
        this.modalSize = ModalSize.Default; 
    }
    
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
    
    createModalElement() {
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

    showIframe(url: string, title: string, size: ModalSize = null) {
        this.modalTitle = title;
        this.modalSize = size ?? ModalSize.Default;
        this.createModalElement();
        const modalBody = this.modalElement.querySelector(".modal-body");
        
        let style = bootstrapVersion == 5 ?"width: 100vw; height: 100vh;" : "width: 100%; height: 100%;";
        
        modalBody.innerHTML = `<iframe src="${url}" frameborder="0" style="${style}"></iframe>`;

        this.showModal();
    }
    
    async showUrl(options: ModalUrlOptions, title: string, size: ModalSize = null) {
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

var defaultModal = function () {
    if (!(this instanceof Modal)) {
        return new Modal();
    }
}();

