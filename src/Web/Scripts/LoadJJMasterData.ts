﻿function loadJJMasterData(event?, prefixSelector?) {

    if (prefixSelector === undefined || prefixSelector === null) {
        prefixSelector = "";
    }
    
    $(prefixSelector + ".selectpicker").selectpicker({
        iconBase: 'fa'
    });
    
    $(prefixSelector + "input[type=checkbox][data-toggle^=toggle]").bootstrapToggle();
    
    $(prefixSelector + ".jjform-datetime").flatpickr({
        enableTime: true,
        wrap: true,
        allowInput: true,
        altInput: false,
        time_24hr: true,
        dateFormat: localeCode === "pt" ? "d/m/Y H:i" : "m/d/Y H:i",
        onOpen: function (selectedDates, dateStr, instance) {
            if(instance.input.getAttribute("autocompletePicker") == 1){
                instance.setDate(Date.now())
            }
        },
        locale: localeCode
    });

    $(prefixSelector + ".jjform-date").flatpickr({
        enableTime: false,
        wrap: true,
        allowInput: true,
        altInput: false,
        dateFormat: localeCode === "pt" ? "d/m/Y" : "m/d/Y",
        onOpen: function (selectedDates, dateStr, instance) {
            if(instance.input.getAttribute("autocompletePicker") == 1){
                instance.setDate(Date.now())
            }
        },
        locale: localeCode
    });
    
    $(prefixSelector + ".jjform-hour").flatpickr({
        enableTime: true,
        wrap: true,
        noCalendar: true,
        allowInput: true,
        altInput: false,
        dateFormat: "H:i",
        time_24hr: true,
        onOpen: function (selectedDates, dateStr, instance) {
            if(instance.input.getAttribute("autocompletePicker") == 1){
                instance.setDate(Date.now())
            }
        },
        locale: localeCode
    });

    $(prefixSelector + ".jjdecimal").each(applyDecimalPlaces);

    $(prefixSelector + "[data-toggle='tooltip'], " + prefixSelector + "[data-bs-toggle='tooltip']").tooltip({
        container: "body",
        trigger: "hover"
    });

    TextArea.setup();
    SearchBox.setup();
    Lookup.setup();
    JJSortable.setup();
    UploadArea.setup();
    TabNav.setup();

    Slider.observeSliders();
    Slider.observeInputs();

    $(document).on({
        ajaxSend: function (event, jqXHR, settings) {
            if (settings.url != null &&
                settings.url.indexOf("t=jjsearchbox") !== -1) {
                return null;
            }

            if (showWaitOnPost) {
                SpinnerOverlay.show();
            }
        },
        ajaxStop: function () { SpinnerOverlay.hide(); }
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
            setTimeout(function () { SpinnerOverlay.show(); }, 1);
        }
    });
}