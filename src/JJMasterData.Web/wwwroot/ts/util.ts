var jjutil = (function () {

    return {
        /**
         * Habilita apenas números inteiros
         *
         * @param {Event} e Event
         * @returns {boolean} caracter válido ou não
         */
        justNumber: function (e) {
            var strCheck = "0123456789";
            var key = "";

            let whichCode;

            if (e.which != null)
                whichCode = e.which;
            else if (e.keyCode != null)
                whichCode = e.keyCode;

            if (whichCode == 8) return true; //backspace
            if (whichCode == 0) return true; //del

            //Ctrl+C,Ctrl+X,Ctrl+V,Ctrl+A
            if ((whichCode == 99 || whichCode == 97 ||
                whichCode == 118 || whichCode == 120 ||
                whichCode == 67 || whichCode == 88 ||
                whichCode == 86 || whichCode == 65) &&
                (e.ctrlKey === true || e.metaKey === true)) {
                return true;
            }

            key = String.fromCharCode(whichCode);
            if (strCheck.indexOf(key) == -1) return false;

        },

        /**
         * Habilita apenas números com decimais
         *
         * @param {Event} e Event
         * @returns {boolean} caracter válido ou não
         */
        justDecimal: function (e) {
            var strCheck = "-0123456789.,";
            var key = "";

            let whichCode;

            if (e.which != null)
                whichCode = e.which;
            else if (e.keyCode != null)
                whichCode = e.keyCode;


            if (whichCode == 8) return true; //backspace
            if (whichCode == 0) return true; //del

            //Ctrl+C,Ctrl+X,Ctrl+V,Ctrl+A
            if ((whichCode == 99 || whichCode == 120 || whichCode == 118 || whichCode == 97 ||
                whichCode == 67 || whichCode == 88 || whichCode == 86 || whichCode == 65) &&
                (e.ctrlKey === true || e.metaKey === true)) {
                return true;
            }

            key = String.fromCharCode(whichCode);
            if (strCheck.indexOf(key) == -1) return false;
        },

        /**
         * Posiciona o foco no proximo elemento
         *
         * @param {string} currentId id do campo atual
         */
        gotoNextFocus: function (currentId) {
            var self = $("#" + currentId);
            var form = self.parents("form:eq(0)");
            var focusable = form.find("input,a.btn,select,button[type=submit]").filter(":visible");
            var next = focusable.eq(focusable.index(self) + 1);
            if (next.length) {
                //if disable try get next 10 fields
                if (next.is(":disabled")) {
                    for (let i = 2; i < 1000; i++) {
                        next = focusable.eq(focusable.index(self) + i);
                        if (!next.is(":disabled") && next.is(":visible"))
                            break;
                    }
                }
                next.focus();
                next.select();
            }
        },

        replaceEntertoTab: function (objid) {
            $("#" + objid + " input").on("keypress", function (e) {
                //enter pressed
                if (e.keyCode == 13) {
                    jjutil.gotoNextFocus($(this).attr("id"));
                    return false;
                }
            });
        }


    };

})();