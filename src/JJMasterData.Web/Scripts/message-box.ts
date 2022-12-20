var messageBox = (function () {

    const TMessageIcon = {
        NONE: 1,
        INFO: 2,
        WARNING: 3,
        ERROR: 4,
        QUESTION: 5
    };

    const TMessageSize = {
        SMALL: 1,
        DEFAULT: 2,
        LARGE: 3
    };

    var jQueryModalId = "#site-modal";
    var jQueryModalTitleId = "#site-modal-title";
    var jQueryModalContentId = "#site-modal-content";
    var jQueryModalButton1Id = "#site-modal-btn1";
    var jQueryModalButton2Id = "#site-modal-btn2";

    var modalId = jQueryModalId.substring(1);
    var button1Id = jQueryModalButton1Id.substring(1);

    function setTitle(title) {
        $(jQueryModalTitleId).html(title);
    }

    function setContent(content) {
        $(jQueryModalContentId).html(content);
    }

    function showModal() {

        if (bootstrapVersion < 5) {
            $(jQueryModalId)
                .modal()
                .on("shown.bs.modal", function () {
                    $(jQueryModalButton1Id).focus();
                });
        }
        else {
            const modal = new bootstrap.Modal(document.getElementById(modalId), {});
            modal.show();

            // @ts-ignore
            modal.addEventListener('shown.bs.modal', function () {
                document.getElementById(button1Id).focus();
            });

        }

    }

    function setBtn1(label, func) {
        $(jQueryModalButton1Id).text(label);
        if ($.isFunction(func)) {
            $(jQueryModalButton1Id).on("click.siteModalClick1", func);
        }
        $(jQueryModalButton1Id).show();
    }

    function setBtn2(label, func) {
        $(jQueryModalButton2Id).text(label);
        if ($.isFunction(func)) {
            $(jQueryModalButton2Id).on("click.siteModalClick2", func);
        }
        $(jQueryModalButton2Id).show();
    }

    function reset() {
        setTitle("");
        setContent("");

        $(jQueryModalButton1Id).text("");
        $(jQueryModalButton1Id).off("click.siteModalClick1");
        $(jQueryModalButton2Id).text("");
        $(jQueryModalButton2Id).off("click.siteModalClick2");
    }

    function loadHtml(icontype, sizetype) {
        if ($(jQueryModalId).length) {
            $(jQueryModalId).remove();
        }
        var html = "";
        html += "<div id=\"site-modal\" tabindex=\"-1\" class=\"modal fade\" role=\"dialog\">\r\n";
        html += "  <div class=\"modal-dialog";
        if (sizetype == TMessageSize.LARGE)
            html += " modal-lg";
        else if (sizetype == TMessageSize.SMALL)
            html += " modal-sm";
        html += "\" role=\"document\">\r\n";
        html += "    <div class=\"modal-content\">\r\n";
        html += "      <div class=\"modal-header\">\r\n";

        if (bootstrapVersion >= 4) {
            html += "        <h4 id=\"site-modal-title\" class=\"modal-title\"></h4>\r\n";
        } else if (bootstrapVersion >= 5) {
            html += "        <button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"modal\"></button>\r\n";
        } else if (bootstrapVersion == 3) {
            html += "        <h4 id=\"site-modal-title\" class=\"modal-title\"><button type=\"button\" class=\"close\" data-dismiss=\"modal\">&times;</button></h4>\r\n";
        }

        html += "      </div>\r\n";
        html += "      <div class=\"modal-body\">\r\n";
        html += "        <table border=\"0\">\r\n";
        html += "          <tr>\r\n";
        html += "            <td style=\"width:40px\">\r\n";

        if (icontype == TMessageIcon.ERROR) {
            html += "              <span class=\"text-danger\">\r\n";
            html += "                <span class=\"fa fa-times-circle\" aria-hidden=\"true\" style=\"font-size: 30px;\"></span>\r\n";
            html += "              </span>\r\n";
        } else if (icontype == TMessageIcon.WARNING) {
            html += "              <span class=\"text-warning\">\r\n";
            html += "                <span class=\"fa fa-exclamation-triangle \" aria-hidden=\"true\" style=\"font-size: 30px;\"></span>\r\n";
            html += "              </span>\r\n";
        } else if (icontype == TMessageIcon.INFO) {
            html += "              <span class=\"text-info\">\r\n";
            html += "                <span class=\"fa fa-info-circle\" aria-hidden=\"true\" style=\"font-size: 30px;\"></span>\r\n";
            html += "              </span>\r\n";
        } else if (icontype == TMessageIcon.QUESTION) {
            html += "              <span class=\"text-info\">\r\n";
            html += "                <span class=\"fa fa-question-circle\" aria-hidden=\"true\" style=\"font-size: 30px;\"></span>\r\n";
            html += "              </span>\r\n";
        }
        html += "            </td>\r\n";
        html += "            <td>\r\n";
        html += "              <span id=\"site-modal-content\"></span>\r\n";
        html += "            </td>\r\n";
        html += "          </tr>\r\n";
        html += "        </table>\r\n";

        html += "      </div>\r\n";
        html += "      <div class=\"modal-footer\">\r\n";
        if (bootstrapVersion == 3) {
            html += "        <button type=\"button\" id=\"site-modal-btn1\" class=\"btn btn-default\" data-dismiss=\"modal\"></button>\r\n";
            html += "        <button type=\"button\" id=\"site-modal-btn2\" class=\"btn btn-default\" data-dismiss=\"modal\"></button>\r\n";
        }
        else if (bootstrapVersion == 4) {
            html += "        <button type=\"button\" id=\"site-modal-btn1\" class=\"btn btn-outline-dark\" data-dismiss=\"modal\"></button>\r\n";
            html += "        <button type=\"button\" id=\"site-modal-btn2\" class=\"btn btn-outline-dark\" data-dismiss=\"modal\"></button>\r\n";
        }
        else {
            html += "        <button type=\"button\" id=\"site-modal-btn1\" class=\"btn btn-outline-dark\" data-bs-dismiss=\"modal\"></button>\r\n";
            html += "        <button type=\"button\" id=\"site-modal-btn2\" class=\"btn btn-outline-dark\" data-bs-dismiss=\"modal\"></button>\r\n";
        }
        html += "      </div>\r\n";
        html += "    </div>\r\n";
        html += "  </div>\r\n";
        html += "</div>\r\n";
        $("body").append(html);
    }

    return {
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
         * @author Lucio Pelinson
         * @since 26/03/2017
         */
        show: function (title, content, icontype, sizetype?, btn1Label?, btn1Func?, btn2Label?, btn2Func?) {
            reset();
            loadHtml(icontype, sizetype);
            setTitle(title);
            setContent(content);

            if (btn1Label === undefined) {
                setBtn1("Fechar", null);
            }
            else {
                setBtn1(btn1Label, btn1Func);
            }

            if (btn2Label === undefined) {
                $(jQueryModalButton2Id).hide();
            }
            else {
                setBtn2(btn2Label, btn2Func);
            }

            showModal();
        },
        hide: function () {
            $(jQueryModalId).modal("hide");
            $(".modal-backdrop").hide();
        }
    };
})();