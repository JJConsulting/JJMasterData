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
         * Focus to the next element, like the tab key
         */
        gotoNextFocus: function (currentId) {
            const self = $("#" + currentId);
            const form = self.parents("form:eq(0)");
            const focusable = form.find("input,a.btn,textarea,select,button")
                .filter(":visible")
                .not(".input-group a.btn")
                .not(".input-group button");            
            let next = focusable.eq(focusable.index(self) + 1);
            if (next.length) {
                if (next.is(":disabled")) {
                    for (let i = 2; i < 1000; i++) {
                        next = focusable.eq(focusable.index(self) + i);
                        if (!next.is(":disabled")  && !next.hasClass("disabled") && next.is(":visible"))
                            break;
                    }
                }
                next.trigger("focus");
                next.trigger("select");
            }
        },



        replaceEntertoTab: function (objid) {
            $("#" + objid + " input, #" + objid + " select").on("keypress change", function (e) {
                if (e.type === "keypress" && e.keyCode === 13) {
                    jjutil.gotoNextFocus($(this).attr("id"));
                    return false;
                }
            });
        },

        animateValue: function (id:string, start:number, end:number, duration:number) {
            if (start === end) return;
            var range = end - start;
            var current = start;
            var increment = end > start ? 1 : -1;
            var stepTime = Math.abs(Math.floor(duration / range));

            var incrementValue = increment;
            if (stepTime == 0) {
                incrementValue = increment * Math.abs(Math.ceil(range / duration));
                stepTime = 1;
            }
                
            var obj = document.getElementById(id);
            var timer = setInterval(function () {
                current = parseInt(obj.innerHTML);
                current += incrementValue;
                if ((current >= end && increment > 0) ||
                    (current <= end && increment < 0)) {
                    obj.innerHTML = end.toString();
                    clearInterval(timer);
                } else {
                    obj.innerHTML = current.toString();
                }

            }, stepTime);
        }

    };
    
})();

function printTemplateIframe() {
    // @ts-ignore
    const iframe: HTMLIFrameElement = document.getElementById('jjmasterdata-template-iframe');
    if (iframe) {
        iframe.contentWindow.document.title = document.title;
        iframe.contentWindow.focus();
        iframe.contentWindow.print();
    }
}

function submitParentWindow() {
    // @ts-ignore
    window.parent.getMasterDataForm().submit();
}

const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

function onDOMReady(callback) {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', callback);
    } else {
        callback();
    }
}

const iconsModal = new Modal();
iconsModal.modalId = "icons-modal";
iconsModal.centered = true;