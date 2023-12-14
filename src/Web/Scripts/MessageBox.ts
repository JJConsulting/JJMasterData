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

    private static loadHtml(icontype: TMessageIcon, sizetype: TMessageSize): void {
        if ($(MessageBox.jQueryModalId).length) {
            $(MessageBox.jQueryModalId).remove();
        }
        let html = "";
        html += "<div id=\"site-modal\" tabindex=\"-1\" class=\"modal fade\" role=\"dialog\">\r\n";
        html += "  <div class=\"modal-dialog";
        if (sizetype == TMessageSize.LARGE) html += " modal-lg";
        else if (sizetype == TMessageSize.SMALL) html += " modal-sm";
        html += "\" role=\"document\">\r\n";
        html += "    <div class=\"modal-content\">\r\n";
        html += "      <div class=\"modal-header\">\r\n";

        if (bootstrapVersion >= 4) {
            html += "        <h4 id=\"site-modal-title\" class=\"modal-title\"></h4>\r\n";
        } else if (bootstrapVersion >= 5) {
            html +=
                '        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>\r\n';
        } else if (bootstrapVersion == 3) {
            html +=
                '        <h4 id="site-modal-title" class="modal-title"><button type="button" class="close" data-dismiss="modal">&times;</button></h4>\r\n';
        }

        html += "      </div>\r\n";
        html += "      <div class=\"modal-body\">\r\n";
        html += "        <table border=\"0\">\r\n";
        html += "          <tr>\r\n";
        html += '            <td style="width:40px">\r\n';

        if (icontype == TMessageIcon.ERROR) {
            html += '              <span class="text-danger">\r\n';
            html +=
                '                <span class="fa fa-times-circle" aria-hidden="true" style="font-size: 30px;"></span>\r\n';
            html += "              </span>\r\n";
        } else if (icontype == TMessageIcon.WARNING) {
            html += '              <span class="text-warning">\r\n';
            html +=
                '                <span class="fa fa-exclamation-triangle " aria-hidden="true" style="font-size: 30px;"></span>\r\n';
            html += "              </span>\r\n";
        } else if (icontype == TMessageIcon.INFO) {
            html += '              <span class="text-info">\r\n';
            html +=
                '                <span class="fa fa-info-circle" aria-hidden="true" style="font-size: 30px;"></span>\r\n';
            html += "              </span>\r\n";
        } else if (icontype == TMessageIcon.QUESTION) {
            html += '              <span class="text-info">\r\n';
            html +=
                '                <span class="fa fa-question-circle" aria-hidden="true" style="font-size: 30px;"></span>\r\n';
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
        } else if (bootstrapVersion == 4) {
            html += '        <button type="button" id="site-modal-btn1" class="btn btn-secondary" data-dismiss="modal"></button>\r\n';
            html += '        <button type="button" id="site-modal-btn2" class="btn btn-secondary" data-dismiss="modal"></button>\r\n';
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

    /**
     * Exibe uma caixa de mensagem (dialog)
     *
     * @param {string} title Título da mensagem
     * @param {string} content Mensagem
     * @param {TMessageIcon} icontype [opcional] Tipo do ícone 1=NONE(padrão), 2=INFO, 3=WARNING, 4=ERROR
     * @param {TMessageSize} sizetype [opcional] Tamanho 1=SMALL, 2=DEFAULT(padrão), 3=LARGE
     * @param {string} btn1Label [opcional] Descrição do btn1 padrão = 'Fechar'
     * @param {Function} btn1Func [opcional] Evento disparado ao clicar no btn1
     * @param {string} btn2Label [opcional] Descrição do btn2
     * @param {Function} btn2Func [opcional] Evento disparado ao clicar no btn2
     * @memberof MessageBox
     */
    public static show(
        title: string,
        content: string,
        icontype: TMessageIcon,
        sizetype?: TMessageSize,
        btn1Label?: string,
        btn1Func?: (() => void) | null,
        btn2Label?: string,
        btn2Func?: (() => void) | null
    ): void {
        MessageBox.reset();
        MessageBox.loadHtml(icontype, sizetype || TMessageSize.DEFAULT);
        MessageBox.setTitle(title);
        MessageBox.setContent(content);

        if (btn1Label === undefined) {
            MessageBox.setBtn1("Fechar", null);
        } else {
            MessageBox.setBtn1(btn1Label, btn1Func);
        }

        if (btn2Label === undefined) {
            $(MessageBox.jQueryModalButton2Id).hide();
        } else {
            MessageBox.setBtn2(btn2Label, btn2Func);
        }

        MessageBox.showModal();
    }

    public static hide(): void {
        $(MessageBox.jQueryModalId).modal("hide");
        $(".modal-backdrop").hide();
    }
}

// Maintain compatibility with the global variable
const messageBox = MessageBox;