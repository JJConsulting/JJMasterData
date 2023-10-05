const listenAllEvents = (selectorPrefix: string = String()) => {
    selectorPrefix += " "
    
    $(selectorPrefix + ".selectpicker").selectpicker({
        iconBase: 'fa'
    });
    
    if(bootstrapVersion === 3){
        $(selectorPrefix + "input[type=checkbox][data-toggle^=toggle]").bootstrapToggle();
    }
    
    CalendarListener.listen(selectorPrefix);
    TextAreaListener.listenKeydown(selectorPrefix);
    SearchBoxListener.listenTypeahed(selectorPrefix);
    LookupListener.listenChanges(selectorPrefix);
    SortableListener.listenSorting(selectorPrefix);
    UploadAreaListener.listenFileUpload(selectorPrefix);
    TabNavListener.listenTabNavs(selectorPrefix);
    SliderListener.listenSliders(selectorPrefix);
    SliderListener.listenInputs(selectorPrefix);
    
    //@ts-ignore
    Inputmask().mask(document.querySelectorAll("input"));
    
    if(bootstrapVersion === 5){
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
        tooltipTriggerList.forEach(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl,{   trigger : 'hover'}))
    }
    
    $(document).on({
        ajaxSend: function (event, jqXHR, settings) {
            if (settings.url != null &&
                settings.url.indexOf("context=searchBox") !== -1) {
                return null;
            }

            if (showSpinnerOnPost) {
                SpinnerOverlay.show();
            }
        },
        ajaxStop: function () { SpinnerOverlay.hide(); }
    });
    
    document.querySelector("form").addEventListener("submit", function (event) {
        let isValid: boolean;

        if (typeof this.reportValidity === "function") {
            isValid = this.reportValidity();
        } else {
            isValid = true;
        }

        if (isValid && showSpinnerOnPost) {
            setTimeout(function () {
                SpinnerOverlay.show();
            }, 1);
        }
    });
};   
