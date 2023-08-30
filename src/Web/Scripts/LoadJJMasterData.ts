function loadJJMasterData(event?, prefixSelector?) {

    if (prefixSelector === undefined || prefixSelector === null) {
        prefixSelector = "";
    }
    
    $(prefixSelector + ".selectpicker").selectpicker({
        iconBase: 'fa'
    });
    
    $(prefixSelector + "input[type=checkbox][data-toggle^=toggle]").bootstrapToggle();
    
    CalendarListener.listen(prefixSelector);

    TextAreaListener.listenKeydown();
    SearchBoxListener.listenTypeahed();
    LookupListener.listenChanges();
    SortableListener.listenSorting();
    UploadAreaListener.listenFileUpload();
    TabNavListener.listenTabNavs();
    SliderListener.listenSliders();
    SliderListener.listenInputs();

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
}
