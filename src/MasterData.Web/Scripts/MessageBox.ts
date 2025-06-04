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
        if (bootstrapVersion < 5) {
            $(MessageBox.jQueryModalId)
                .modal()
                .on("shown.bs.modal", function () {
                    $(MessageBox.jQueryModalButton1Id).focus();
                });
        } else {
            const modal = bootstrap.Modal.getOrCreateInstance(
                document.getElementById(MessageBox.modalId),
                {}
            );
            modal.show();

            document.addEventListener("shown.bs.modal", function () {
                document.getElementById(MessageBox.button1Id).focus();
            });
        }
    }

    private static setBtn1(label: string, func: (() => void) | null): void {
        $(MessageBox.jQueryModalButton1Id).text(label);
        if ($.isFunction(func)) {
            $(MessageBox.jQueryModalButton1Id).on("click.siteModalClick1", func);
        }
        $(MessageBox.jQueryModalButton1Id).show();
    }

    private static setBtn2(label: string, func: (() => void) | null): void {
        $(MessageBox.jQueryModalButton2Id).text(label);
        if ($.isFunction(func)) {
            $(MessageBox.jQueryModalButton2Id).on("click.siteModalClick2", func);
        }
        $(MessageBox.jQueryModalButton2Id).show();
    }

    private static reset(): void {
        MessageBox.hide()
    }

    private static loadHtml(hasTitle: boolean, iconType: TMessageIcon, iconSize: TMessageSize, allowClose: boolean): void {
        if ($(MessageBox.jQueryModalId).length) {
            $(MessageBox.jQueryModalId).remove();
        }

        let html = "";
        html += `<div id=\"site-modal\" tabindex=\"-1\" ${allowClose ? "data-bs-backdrop='static' data-bs-keyboard='false'" : ""} class=\"modal fade\" role=\"dialog\">\r\n`;
        html += "  <div class=\"modal-dialog";
        if (iconSize == TMessageSize.LARGE) html += " modal-lg";
        else if (iconSize == TMessageSize.SMALL) html += " modal-sm";
        html += "\" role=\"document\">\r\n";
        html += "    <div class=\"modal-content\">\r\n";
        html += "      <div class=\"modal-header\">\r\n";

        if (bootstrapVersion >= 4 && hasTitle) {
            html += "        <h4 id=\"site-modal-title\" class=\"modal-title\"></h4>\r\n";
        } else if (bootstrapVersion >= 5) {
            html +=
                '        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>\r\n';
        } else if (bootstrapVersion == 3 && hasTitle) {
            html +=
                '        <h4 id="site-modal-title" class="modal-title"><button type="button" class="close" data-dismiss="modal">&times;</button></h4>\r\n';
        }

        html += "      </div>\r\n";
        html += "      <div class=\"modal-body\">\r\n";
        html += "        <table border=\"0\">\r\n";
        html += "          <tr>\r\n";
        html += '            <td style="width:40px">\r\n';

        if (iconType == TMessageIcon.ERROR) {
            html += '              <span class="text-danger">\r\n';
            html +=
                '                <span class="fa fa-times-circle" aria-hidden="true" style="font-size: 1.875rem;"></span>\r\n';
            html += "              </span>\r\n";
        } else if (iconType == TMessageIcon.WARNING) {
            html += '              <span class="text-warning">\r\n';
            html +=
                '                <span class="fa fa-exclamation-triangle " aria-hidden="true" style="font-size: 1.875rem;"></span>\r\n';
            html += "              </span>\r\n";
        } else if (iconType == TMessageIcon.INFO) {
            html += '              <span class="text-info">\r\n';
            html +=
                '                <span class="fa fa-info-circle" aria-hidden="true" style="font-size:1.875rem;"></span>\r\n';
            html += "              </span>\r\n";
        } else if (iconType == TMessageIcon.QUESTION) {
            html += '              <span class="text-info">\r\n';
            html +=
                '                <span class="fa fa-question-circle" aria-hidden="true" style="font-size:1.875rem;"></span>\r\n';
            html += "              </span>\r\n";
        }
        html += "            </td>\r\n";
        html += "            <td>\r\n";
        html += '              <span id="site-modal-content"></span>\r\n';
        html += "            </td>\r\n";
        html += "          </tr>\r\n";
        html += "        </table>\r\n";

        html += "      </div>\r\n";
        html += "      <div class=\"modal-footer\">\r\n";
        if (bootstrapVersion == 3) {
            html += '        <button type="button" id="site-modal-btn1" class="btn btn-default" data-dismiss="modal"></button>\r\n';
            html += '        <button type="button" id="site-modal-btn2" class="btn btn-default" data-dismiss="modal"></button>\r\n';
        } else {
            html += '        <button type="button" id="site-modal-btn1" class="btn btn-secondary" data-bs-dismiss="modal"></button>\r\n';
            html += '        <button type="button" id="site-modal-btn2" class="btn btn-secondary" data-bs-dismiss="modal"></button>\r\n';
        }
        html += "      </div>\r\n";
        html += "    </div>\r\n";
        html += "  </div>\r\n";
        html += "</div>\r\n";
        $("body").append(html);
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
        MessageBox.reset();
        MessageBox.loadHtml((title != null && title != ""), iconType, sizeType || TMessageSize.DEFAULT, btn1Callback != null || btn2Callback != null);
        MessageBox.setTitle(title)
        MessageBox.setContent(description);

        if (btn1Label === undefined) {
            MessageBox.setBtn1(Localization.get("Close"), null);
        } else {
            MessageBox.setBtn1(btn1Label, btn1Callback);
        }

        if (btn2Label === undefined) {
            $(MessageBox.jQueryModalButton2Id).hide();
        } else {
            MessageBox.setBtn2(btn2Label, btn2Callback);
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
        return new Promise<boolean>((resolve, reject) => {
            MessageBox.showConfirmationDialog({
                description: message,
                cancelLabel: Localization.get('No'),
                confirmLabel: Localization.get('Yes'),
                confirmCallback: () => {
                    MessageBox.hide();
                    resolve(true);
                },
                cancelCallback: () => {
                    MessageBox.hide();
                    resolve(false);
                },
            });
        });
    }

    public static hide(): void {
        $(MessageBox.jQueryModalId).modal("hide");
        $(".modal-backdrop").hide();
    }
}

// Maintain compatibility with the global variable
const messageBox = MessageBox;

const showConfirmationDialog = MessageBox.showConfirmationDialog;
const showConfirmationMessage = MessageBox.showConfirmationMessage;