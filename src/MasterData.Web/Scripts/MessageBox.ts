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
        const parent = Array.from(document.querySelectorAll<HTMLElement>(".modal.show"))
            .filter(modal => modal !== element)
            .sort((a, b) => Number(getComputedStyle(b).zIndex) - Number(getComputedStyle(a).zIndex))[0];
        const previousFocus = document.activeElement as HTMLElement;
        const bodyOverflow = document.body.style.overflow;
        const bodyPadding = document.body.style.paddingRight;
        const backdrops = new Set(document.querySelectorAll(".modal-backdrop"));

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

        $(element).one("shown.bs.modal", () => document.getElementById(MessageBox.button1Id).focus());
        bootstrap.Modal.getOrCreateInstance(element).show();

        if (parent) {
            const backdrop = Array.from(document.querySelectorAll<HTMLElement>(".modal-backdrop"))
                .find(candidate => !backdrops.has(candidate));
            if (backdrop) backdrop.style.zIndex = String(Number(element.style.zIndex) - 1);
        }
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
        const header = hasTitle
            ? '<h4 id="site-modal-title" class="modal-title"></h4>'
            : '<button type="button" class="btn-close" data-bs-dismiss="modal"></button>';

        $("body").append(`
            <div id="site-modal" tabindex="-1" class="modal fade" role="dialog"
                 ${allowClose ? 'data-bs-backdrop="static" data-bs-keyboard="false"' : ""}>
                <div class="modal-dialog ${sizeClass}" role="document">
                    <div class="modal-content">
                        <div class="modal-header">${header}</div>
                        <div class="modal-body">
                            <table border="0"><tr>
                                <td style="width:40px">
                                    ${icon ? `<span class="${icon[0]}"><span class="fa ${icon[1]}" aria-hidden="true" style="font-size:1.875rem;"></span></span>` : ""}
                                </td>
                                <td><span id="site-modal-content"></span></td>
                            </tr></table>
                        </div>
                        <div class="modal-footer">
                            <button type="button" id="site-modal-btn1" class="btn btn-secondary" data-bs-dismiss="modal"></button>
                            <button type="button" id="site-modal-btn2" class="btn btn-secondary" data-bs-dismiss="modal"></button>
                        </div>
                    </div>
                </div>
            </div>`);
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