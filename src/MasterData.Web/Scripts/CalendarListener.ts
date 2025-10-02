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
                disableMobile: true,
                time_24hr: true,
                mode: "single",
                //@ts-ignore
                dateFormat: div.children[0].dataset.flatpickrMask,
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
                disableMobile: true,
                monthSelectorType: 'static',
                mode: "single",
                //@ts-ignore
                dateFormat: div.children[0].dataset.flatpickrMask,
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