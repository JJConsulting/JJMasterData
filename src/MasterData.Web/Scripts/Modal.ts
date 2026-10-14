enum ModalSize {
    Fullscreen,
    ExtraLarge,
    Large,
    Default,
    Small
}

class ModalUrlOptions {
    url: string
    requestOptions?: RequestInit
}

class Modal {
    modalId = "jjmasterdata-modal";
    modalElement: HTMLElement;
    modalSize = ModalSize.ExtraLarge;
    centered: boolean;

    private modalSizeCssClass = {
        Default: "jj-modal-default",
        ExtraLarge: "jj-modal-xl",
        Large: "jj-modal-lg",
        Small: "jj-modal-sm",
        Fullscreen: "modal-fullscreen",
    };

    private _modalTitle: string;

    get modalTitle(): string {
        return this._modalTitle;
    }

    set modalTitle(value: string) {
        this._modalTitle = value;
        document.getElementById(`${this.modalId}-title`).innerText = value;
    }


    private getModalCssClass() {
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
            <h5 class="modal-title" id="${this.modalId}-title">${this.modalTitle}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body"> </div>
        </div>
      </div>`;
            let form = getMasterDataForm();
            if (form) {
                form.appendChild(this.modalElement);
            } else {
                document.body.appendChild(this.modalElement);
            }

            this.modalElement.addEventListener('hidden.bs.modal', event => {
                if (event.target === this.modalElement) this.modalElement.remove();
            });

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

    showIframe(url: string, title: string, size: ModalSize = null) {
        this._modalTitle = title;
        this.modalSize = size ?? ModalSize.Default;
        this.createModalElement();
        const modalBody = this.modalElement.querySelector(".modal-body");

        modalBody.innerHTML = `<iframe src="${url}" class="modal-iframe"></iframe>`;

        Modal.showElement(this.modalElement);
    }

    async showUrl(modalOptions: ModalUrlOptions | string, title: string, size: ModalSize = null) {
        this._modalTitle = title;
        this.modalSize = size ?? ModalSize.Default;
        this.createModalElement();

        let fetchUrl: string;
        let fetchOptions: RequestInit;

        if (typeof modalOptions === 'object' && 'url' in modalOptions) {
            fetchUrl = modalOptions.url;
            fetchOptions = modalOptions.requestOptions;
        } else {
            fetchUrl = modalOptions;
        }

        return await fetch(fetchUrl, fetchOptions)
            .then(async response => {
                if (response.headers.get("content-type")?.includes("application/json")) {
                    return response.json();
                } else if (response.redirected) {
                    window.open(response.url, '_blank').focus();
                } else {
                    return response.text().then((htmlData) => {
                        this.setAndShowModal(htmlData)
                    });
                }
            })
    }

    private setAndShowModal(content: string) {
        const modalBody = this.modalElement.querySelector<HTMLElement>(`#${this.modalId} .modal-body`);
        HTMLHelper.setInnerHTML(modalBody, content)

        Modal.showElement(this.modalElement);
    }

    hide() {
        bootstrap.Modal.getOrCreateInstance("#" + this.modalId).hide();
    }

    remove() {
        this.hide();
        document.getElementById(this.modalId).remove();
    }

    static showElement(element: HTMLElement): void {
        bootstrap.Modal.getOrCreateInstance(element).show();
    }

    static prepareElement(element: HTMLElement): void {
        const parent = Array.from(document.querySelectorAll<HTMLElement>(".modal.show"))
            .filter(modal => modal !== element)
            .sort((a, b) => Number(getComputedStyle(b).zIndex) - Number(getComputedStyle(a).zIndex))[0];
        const previousFocus = document.activeElement as HTMLElement;
        const bodyOverflow = document.body.style.overflow;
        const bodyPadding = document.body.style.paddingRight;
        const backdrops = new Set(document.querySelectorAll(".modal-backdrop"));

        const originalParent = element.parentElement;
        if (originalParent?.closest(".modal")) {
            // Keep nested dialogs outside transformed modal content and inside the form.
            (getMasterDataForm() ?? document.body).appendChild(element);
            element.addEventListener("hidden.bs.modal", () => {
                // Restore only after the event has finished bubbling to delegated close handlers.
                Promise.resolve().then(() => {
                    if (originalParent.isConnected) originalParent.appendChild(element);
                    else element.remove();
                });
            }, {once: true});
        }

        if (parent) {
            element.style.zIndex = String(Number(getComputedStyle(parent).zIndex) + 20);
            // Bootstrap owns a single focus trap; suspend it while the child modal is open.
            bootstrap.Modal.getInstance(parent)?._focustrap.deactivate();

            $(element).one("hidden.bs.modal", () => {
                if (!parent.isConnected || !parent.classList.contains("show"))
                    return;
                document.body.classList.add("modal-open");
                document.body.style.overflow = bodyOverflow;
                document.body.style.paddingRight = bodyPadding;
                bootstrap.Modal.getInstance(parent)?._focustrap.activate();
                if (previousFocus?.isConnected) previousFocus.focus();
            });
        }

        if (parent) {
            // Bootstrap creates the backdrop after the show event; adjust it before the next paint.
            Promise.resolve().then(() => {
                const backdrop = Array.from(document.querySelectorAll<HTMLElement>(".modal-backdrop"))
                    .find(candidate => !backdrops.has(candidate));
                if (backdrop)
                    backdrop.style.zIndex = String(Number(element.style.zIndex) - 1);
            });
        }
    }

}

// Include dialogs opened by generated scripts, data attributes and Bootstrap's own API.
document.addEventListener("show.bs.modal", event => {
    if (event.target instanceof HTMLElement && event.target.classList.contains("modal"))
        Modal.prepareElement(event.target);
});

// Bootstrap's data API normally closes the active dialog before toggling another one.
document.addEventListener("click", event => {
    if (!(event.target instanceof Element)) return;
    const trigger = event.target.closest<HTMLElement>('[data-bs-toggle="modal"]');
    if (!trigger?.closest(".modal.show") || trigger.matches(":disabled, .disabled")) return;
    const selector = trigger.dataset.bsTarget || trigger.getAttribute("href")?.match(/#.+$/)?.[0];
    const target = selector ? document.querySelector<HTMLElement>(selector) : null;
    if (!target?.classList.contains("modal"))
        return;
    event.preventDefault();
    event.stopPropagation();
    const modal = bootstrap.Modal.getOrCreateInstance(target);
    if (target.classList.contains("show")) modal.hide();
    else modal.show();
}, true);

class DefaultModal {

    static instance: Modal

    static getInstance() {
        if (this.instance === undefined) {
            this.instance = new Modal();
        }
        return this.instance;
    }
}

var defaultModal = DefaultModal.getInstance();

// Compatibility with legacy systems.
class popup {
    //Yes, the parameters are inverted.
    static show(title, url, size = null) {
        defaultModal.showIframe(url, title, size)
    }

    static hide() {
        defaultModal.hide()
    }
}
