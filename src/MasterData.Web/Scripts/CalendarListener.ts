class CalendarListener {
    static listen(prefixSelector = "") {
        const init = (selector, enableTime) => {
            document.querySelectorAll(prefixSelector + selector).forEach(div => {
                const input = div.children[0];

                //@ts-ignore
                const mode = input.dataset.mode ?? "single";
                
                //@ts-ignore
                flatpickr(div, {
                    enableTime,
                    wrap: true,
                    allowInput: mode !== "multiple",
                    altInput: false,
                    disableMobile: true,
                    monthSelectorType: "static",
                    time_24hr: enableTime,
                    mode,
                    //@ts-ignore
                    dateFormat: mode !== "multiple" ? input.dataset.flatpickrMask : "Y-m-d",
                    onOpen: (_, __, instance) => {
                        if (instance.input.getAttribute("autocompletePicker") === "True") {
                            instance.setDate(Date.now());
                        }
                    },
                    locale: localeCode,
                });
            });
        };

        init(".jjform-datetime", true);
        init(".jjform-date", false);
    }
}
