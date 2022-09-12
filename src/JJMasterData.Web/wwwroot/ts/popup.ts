class Popup {
    modalId: string = "popup-modal";
    modalTitleId: string = "popup-modal-title";

    private setTitle(title: string) {
        document.getElementById(this.modalTitleId).innerHTML = title;
    }

    private showModal() {
        if (bootstrapVersion < 5) {
            $("#" + this.modalId).modal();
        }
        else {
            const modal = new bootstrap.Modal(document.getElementById(this.modalId), {});
            modal.show();
        }

        messageWait.show();

        $("iframe").on("load", function () {
            messageWait.hide();
        });
    }

    private loadHtml(url, size) {
        if ($("#" + this.modalId).length) {
            $("#" + this.modalId).remove();
        }

        let width;
        let height;

        if (size === undefined) {
            size = "1";
        }

        size = parseInt(size);

        let modalDialogDiv;

        switch (size) {
            case 1:
                width = "98%";
                height = "92%";
                modalDialogDiv = "<div class=\"modal-dialog\" style=\"margin:0.7em;left:0px;right:0px;top:0px;bottom:0px; position:fixed;width:auto;\">\r\n";

                break;
            case 2:
                width = "auto";
                height = "95%";
                modalDialogDiv = "<div class=\"modal-dialog\" style=\"position: auto; height: 95vh;\">\r\n";

                break;
            case 3:
                width = "auto";
                height = "75%";
                modalDialogDiv = "<div class=\"modal-dialog\" style=\"position: auto; height: 75vh;\">\r\n";

                break;
            default:
                width = "65%";
                height = "80%";
                modalDialogDiv = "<div class=\"modal-dialog\" style=\"position: auto; height: 80vh;width:65%\">\r\n";

                break;
        }

        let modalDialogCss = `
@media (min-width: 576px) {
  .modal-dialog { max-width: none; }
}

.modal-dialog {
  width: ${width};
  height: ${height};
  padding: 0;
}

.modal-content {
  height: 100%;
}
`

        let html = "";
        
        html += "<div id=\"popup-modal\" tabindex=\"-1\" class=\"modal fade\" role=\"dialog\">\r\n";

        if (bootstrapVersion == 3) {
            html += modalDialogDiv;
        }
        else {
            $('head').append(`<style type="text/css">${modalDialogCss}</style>`);
            html += "<div class=\"modal-dialog\">";
        }

        if (bootstrapVersion != 3) {
            html += `    <div class="modal-content">\r\n`;
        }
        else {
            html += `    <div class="modal-content" style="height:100%;width:auto;">\r\n`;
        }

        html += "      <div class=\"modal-header\">\r\n";

        if (bootstrapVersion == 3) {
            html += "        <button type=\"button\" class=\"close\" data-dismiss=\"modal\">&times;</button>\r\n";
            html += "        <h4 id=\"popup-modal-title\" class=\"modal-title\"></h4>\r\n";
        }
        else {
            html += "        <h4 id=\"popup-modal-title\" class=\"modal-title\"></h4>\r\n";
            if (bootstrapVersion >= 5) {
                html += "        <button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"modal\"></button>\r\n";
            }
            else {
                html += "        <button type=\"button\" class=\"close\" data-dismiss=\"modal\">&times;</button>\r\n";
            }
        }

        html += "      </div>\r\n";
        html += "      <div class=\"modal-body\"  style=\"height:90%;width:auto;\">\r\n";

        html += "         <iframe style=\"border: 0px;\" ";
        html += " src='";
        html += url;
        html += "' width='100%' height='97%'>Waiting...</iframe>";

        html += "      </div>\r\n";

        html += "    </div>\r\n";
        html += "  </div>\r\n";
        html += "</div>\r\n";
        $(html).appendTo($("body"));
    }

    show(title, url, size = 4) {
        this.loadHtml(url, size);
        this.setTitle(title);
        this.showModal();
    }

    hide() {
        $("#" + this.modalId).modal("hide");
    }

    modal() {
        return $("#" + this.modalId);
    }
}

var popup = function() {
    if (!(this instanceof Popup)) {
        return new Popup();
    }
}()