enum TMessageIcon {
    NONE = 1,
    INFO = 2,
    WARNING = 3,
    ERROR = 4,
    QUESTION = 5,
}

enum TMessageSize {
    SMALL = 1,
    DEFAULT = 2,
    LARGE = 3,
}

class MessageBox {
    private static readonly jQueryModalId = "#site-modal";
    private static readonly jQueryModalTitleId = "#site-modal-title";
    private static readonly jQueryModalContentId = "#site-modal-content";
    private static readonly jQueryModalButton1Id = "#site-modal-btn1";
    private static readonly jQueryModalButton2Id = "#site-modal-btn2";

    private static readonly modalId = MessageBox.jQueryModalId.substring(1);
    private static readonly button1Id = MessageBox.jQueryModalButton1Id.substring(1);
    
    private static setTitle(title: string): void {
        if(title)
            $(MessageBox.jQueryModalTitleId).html(title);
    }

    private static setContent(content: string): void {
        $(MessageBox.jQueryModalContentId).html(content);
    }

    private static showModal(): void {
        const element = document.getElementById(MessageBox.modalId);
        $(element).one("shown.bs.modal", () => document.getElementById(MessageBox.button1Id).focus());
        Modal.showElement(element);
    }

    private static setButton(selector: string, label: string, callback?: (() => void) | null): void {
        const button = $(selector).text(label).show();
        if (callback) button.on("click.siteModal", callback);
    }

    private static loadHtml(hasTitle: boolean, iconType: TMessageIcon, size: TMessageSize, allowClose: boolean): void {
        $(MessageBox.jQueryModalId).remove();
        const sizeClass = size === TMessageSize.LARGE ? "modal-lg" : size === TMessageSize.SMALL ? "modal-sm" : "";
        const icons = {
            [TMessageIcon.ERROR]: ["text-danger", "fa-times-circle"],
            [TMessageIcon.WARNING]: ["text-warning", "fa-exclamation-triangle"],
            [TMessageIcon.INFO]: ["text-info", "fa-info-circle"],
            [TMessageIcon.QUESTION]: ["text-info", "fa-question-circle"],
        };
        const icon = icons[iconType];
        const modal = Modal.cloneModalTemplate();
        modal.id = MessageBox.modalId;
        const template = document.getElementById("jjmasterdata-messagebox-template") as HTMLTemplateElement;
        modal.querySelector(".modal-body").replaceWith(template.content.cloneNode(true));
        const dialog = modal.querySelector<HTMLElement>(".modal-dialog");
        dialog.classList.remove("modal-dialog-scrollable");
        if (sizeClass) dialog.classList.add(sizeClass);
        if (allowClose) {
            modal.setAttribute("data-bs-backdrop", "static");
            modal.setAttribute("data-bs-keyboard", "false");
        }

        const title = modal.querySelector<HTMLElement>(".modal-title");
        title.id = MessageBox.jQueryModalTitleId.substring(1);
        title.classList.add("h4");
        modal.querySelector(hasTitle ? ".btn-close" : MessageBox.jQueryModalTitleId).remove();
        const iconElement = modal.querySelector<HTMLElement>(".message-box-icon");
        if (icon) {
            iconElement.classList.add(icon[0]);
            iconElement.classList.add(icon[1]);
        } else {
            iconElement.remove();
        }

        document.body.appendChild(modal);
    }

    public static show(
        title: string,
        description: string,
        iconType: TMessageIcon,
        sizeType?: TMessageSize,
        btn1Label?: string,
        btn1Callback?: (() => void) | null,
        btn2Label?: string,
        btn2Callback?: (() => void) | null
    ): void {
        MessageBox.hide();
        MessageBox.loadHtml((title != null && title != ""), iconType, sizeType || TMessageSize.DEFAULT, btn1Callback != null || btn2Callback != null);
        MessageBox.setTitle(title)
        MessageBox.setContent(description);

        MessageBox.setButton(MessageBox.jQueryModalButton1Id, btn1Label ?? Localization.get("Close"), btn1Callback);

        if (btn2Label === undefined) {
            $(MessageBox.jQueryModalButton2Id).hide();
        } else {
            MessageBox.setButton(MessageBox.jQueryModalButton2Id, btn2Label, btn2Callback);
        }

        MessageBox.showModal();
    }
    
    public static showConfirmationDialog(options: {
        description: string,
        cancelLabel?: string,
        cancelCallback?: (() => void) | null
        confirmLabel?: string,
        confirmCallback?: (() => void) | null
    }): void {
        const {
            description,
            cancelLabel,
            cancelCallback,
            confirmLabel,
            confirmCallback
        } = options;
        
        MessageBox.show(
            null,
            description, 
            TMessageIcon.QUESTION,
            TMessageSize.DEFAULT,
            confirmLabel ?? Localization.get("Yes"),
            confirmCallback,
            cancelLabel  ?? Localization.get("No"),
            cancelCallback ?? MessageBox.hide
        )
    }

    public static showConfirmationMessage(message: string): Promise<boolean> {
        return new Promise<boolean>(resolve => {
            let confirmed = false;
            MessageBox.showConfirmationDialog({
                description: message,
                cancelLabel: Localization.get('No'),
                confirmLabel: Localization.get('Yes'),
                confirmCallback: () => {
                    confirmed = true;
                    MessageBox.hide();
                },
                cancelCallback: MessageBox.hide,
            });
            $(MessageBox.jQueryModalId).one("hidden.bs.modal", () => resolve(confirmed));
        });
    }

    public static hide(): void {
        bootstrap.Modal.getInstance(document.getElementById(MessageBox.modalId))?.hide();
    }
}

// Maintain compatibility with the global variable
const messageBox = MessageBox;

const showConfirmationDialog = MessageBox.showConfirmationDialog;
const showConfirmationMessage = MessageBox.showConfirmationMessage;