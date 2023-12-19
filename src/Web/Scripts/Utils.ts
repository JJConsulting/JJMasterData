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
            const element = document.getElementById(currentId);
            if (element) {
                const focusableElements = document.querySelectorAll<HTMLElement>(
                    'input:not([disabled]):not([type="hidden"]), select:not([disabled]), button:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"]):not([disabled]):not([hidden])'
                );
                const currentIndex = Array.from(focusableElements).indexOf(element);
                const nextIndex = (currentIndex + 1) % focusableElements.length;
                const nextElement = focusableElements[nextIndex];
                nextElement.focus();
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

function requestSubmitParentWindow() {
    window.parent.document.forms[0].requestSubmit();
}

const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

function onDOMReady(callback) {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', callback);
    } else {
        callback();
    }
}