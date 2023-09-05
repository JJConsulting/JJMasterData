const listenAllEvents = (selectorPrefix: string = String()) => {
    $(selectorPrefix + ".selectpicker").selectpicker({
        iconBase: 'fa'
    });

    $(selectorPrefix + "input[type=checkbox][data-toggle^=toggle]").bootstrapToggle();

    CalendarListener.listen(selectorPrefix);
    TextAreaListener.listenKeydown(selectorPrefix);
    SearchBoxListener.listenTypeahed(selectorPrefix);
    LookupListener.listenChanges(selectorPrefix);
    SortableListener.listenSorting(selectorPrefix);
    UploadAreaListener.listenFileUpload(selectorPrefix);
    TabNavListener.listenTabNavs(selectorPrefix);
    SliderListener.listenSliders(selectorPrefix);
    SliderListener.listenInputs(selectorPrefix);

    $(document).on({
        ajaxSend: function (event, jqXHR, settings) {
            if (settings.url != null &&
                settings.url.indexOf("context=searchBox") !== -1) {
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
};   
