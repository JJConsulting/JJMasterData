function jjloadform(event?, prefixSelector?) {
    if (prefixSelector === undefined || prefixSelector === null) {
        prefixSelector = "";
    }
    
    $(prefixSelector + ".selectpicker").selectpicker({
        iconBase: 'fa'
    });
    
    $(prefixSelector + "input[type=checkbox][data-toggle^=toggle]").bootstrapToggle();

    const datetimeInputs = document.querySelectorAll(prefixSelector + ".jjform-datetime");

    datetimeInputs.forEach(function(div) {
        //@ts-ignore
        flatpickr(div, {
            enableTime: true,
            wrap: true,
            allowInput: true,
            altInput: false,
            time_24hr: true,
            mode: div.firstElementChild.getAttribute("multipledates") === "1" ? "multiple" : "single",
            dateFormat: localeCode === "pt" ? "d/m/Y H:i" : "m/d/Y H:i",
            onOpen: function(selectedDates, dateStr, instance) {
                if (instance.input.getAttribute("autocompletePicker") === "1") {
                    instance.setDate(Date.now());
                }
            },
            locale: localeCode,
        });
    });
    
    const dateInputs = document.querySelectorAll(prefixSelector + ".jjform-date");

    dateInputs.forEach(function(div) {
        //@ts-ignore
        flatpickr(div, {
            enableTime: false,
            wrap: true,
            allowInput: true,
            altInput: false,
            mode: div.firstElementChild.getAttribute("multipledates") === "1" ? "multiple" : "single",
            dateFormat: localeCode === "pt" ? "d/m/Y" : "m/d/Y",
            onOpen: function(selectedDates, dateStr, instance) {
                if (instance.input.getAttribute("autocompletePicker") === "1") {
                    instance.setDate(Date.now());
                }
            },
            locale: localeCode,
        });
    });

    const hourInputs = document.querySelectorAll(prefixSelector + ".jjform-hour");

    hourInputs.forEach(function(div) {
        //@ts-ignore
        flatpickr(div, {
            enableTime: true,
            wrap: true,
            noCalendar: true,
            allowInput: true,
            altInput: false,
            dateFormat: "H:i",
            mode: div.firstElementChild.getAttribute("multipledates") === "1" ? "multiple" : "single",
            time_24hr: true,
            onOpen: function(selectedDates, dateStr, instance) {
                if (instance.input.getAttribute("autocompletePicker") == 1) {
                    instance.setDate(Date.now());
                }
            },
            locale: localeCode,
        });
    });

    $(prefixSelector + ".jjdecimal").each(applyDecimalPlaces);

    $(prefixSelector + "[data-toggle='tooltip'], " + prefixSelector + "[data-bs-toggle='tooltip']").tooltip({
        container: "body",
        trigger: "hover"
    });

    JJTextArea.setup();
    JJSearchBox.setup();
    JJLookup.setup();
    JJSortable.setup();
    JJUpload.setup();
    JJTabNav.setup();

    JJSlider.observeSliders();
    JJSlider.observeInputs();

    messageWait.hide();

    $(document).on({
        ajaxSend: function (event, jqXHR, settings) {
            if (settings.url != null &&
                settings.url.indexOf("t=jjsearchbox") !== -1) {
                return null;
            }

            if (showWaitOnPost) {
                messageWait.show();
            }
        },
        ajaxStop: function () { messageWait.hide(); }
    });

    $("form").on("submit",function () {
        
        let isValid : boolean;
        
        try {
            isValid = $("form").valid();
        }
        catch {
            isValid = true;
        }
        
        if (isValid && showWaitOnPost) {
            setTimeout(function () { messageWait.show(); }, 1);
        }
    });
}
