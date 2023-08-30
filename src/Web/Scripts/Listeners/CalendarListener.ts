class CalendarListener {
    static listen(prefixSelector){
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
    }
}