class CalendarListener {
    static listen(prefixSelector = String()){
        const datetimeInputs = document.querySelectorAll(prefixSelector + ".jjform-datetime");

        datetimeInputs.forEach(function(div) {
            //@ts-ignore
            flatpickr(div, {
                enableTime: true,
                wrap: true,
                allowInput: true,
                altInput: false,
                monthSelectorType: 'static',
                time_24hr: true,
                mode: div.firstElementChild.getAttribute("multiple-dates") === "True" ? "multiple" : "single",
                dateFormat: localeCode === "pt" ? "d/m/Y H:i" : "m/d/Y H:i",
                onOpen: function(selectedDates, dateStr, instance) {
                    if (instance.input.getAttribute("autocompletePicker") === "True") {
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
                monthSelectorType: 'static',
                mode: div.firstElementChild.getAttribute("multiple-dates") === "True" ? "multiple" : "single",
                dateFormat: localeCode === "pt" ? "d/m/Y" : "m/d/Y",
                onOpen: function(selectedDates, dateStr, instance) {
                    if (instance.input.getAttribute("autocompletePicker") === "True") {
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
                mode: div.firstElementChild.getAttribute("multiple-dates") === "True" ? "multiple" : "single",
                time_24hr: true,
                onOpen: function(selectedDates, dateStr, instance) {
                    if (instance.input.getAttribute("autocompletePicker") === "True") {
                        instance.setDate(Date.now());
                    }
                },
                locale: localeCode,
            });
        });
    }
}