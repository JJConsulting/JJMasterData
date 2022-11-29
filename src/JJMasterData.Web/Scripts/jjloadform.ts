function jjloadform(event?, prefixSelector?) {

    if (prefixSelector === undefined) {
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
            instance.setDate(Date.now())
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
            instance.setDate(Date.now())
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
            instance.setDate(Date.now())
        },
        locale: localeCode
    });

    $(prefixSelector + ".jjdecimal").each(function () {
        let decimalPlaces = $(this).attr("jjdecimalplaces");
        if (decimalPlaces == null)
            decimalPlaces = "2";
        
        if(localeCode==='pt')
            $(this).number(true, decimalPlaces, ",", ".");
        else
            $(this).number(true, decimalPlaces);
    });

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
        if ( $("form").valid() && showWaitOnPost) {
            setTimeout(function () { messageWait.show(); }, 1);
        }
    });
}
