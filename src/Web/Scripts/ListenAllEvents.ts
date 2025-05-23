﻿const listenAllEvents = (selectorPrefix: string = String()) => {
    selectorPrefix += " "
    
    $(selectorPrefix + ".selectpicker").selectpicker({
        iconBase: bootstrapVersion === 5 ? 'fa' : 'glyphicon',
        styleBase: bootstrapVersion === 5 ? "form-select form-dropdown" : "form-control"
    });
    
    if(bootstrapVersion === 3){
        $(selectorPrefix + "input[type=checkbox][data-toggle^=toggle]").bootstrapToggle();
    }
    
    CodeEditor.setup(selectorPrefix);
    CalendarListener.listen(selectorPrefix);
    TextAreaListener.listenKeydown(selectorPrefix);
    SearchBoxListener.listenTypeahead(selectorPrefix);
    LookupListener.listenChanges(selectorPrefix);
    SortableListener.listenSorting(selectorPrefix);
    UploadAreaListener.listenFileUpload(selectorPrefix);
    TabNavListener.listenTabNavs(selectorPrefix);
    SliderListener.listenSliders(selectorPrefix);
    SliderListener.listenInputs(selectorPrefix);
    
    GridViewHelper.setupInfiniteScroll();
    
    //@ts-ignore
    Inputmask().mask(document.querySelectorAll(selectorPrefix+"input"));
    
    if(bootstrapVersion === 5){
        TooltipHelper.listen(selectorPrefix);
    }else{
        $(selectorPrefix + '[data-toggle="tooltip"]').tooltip();
    }
    
    const responsiveTables = document.querySelector<HTMLElement>(selectorPrefix +'.table-responsive');

    if(responsiveTables){
        responsiveTables.addEventListener( 'show.bs.dropdown', () => {
            responsiveTables.style.overflow = 'inherit';
        });
        responsiveTables.addEventListener('hide.bs.dropdown', () => {
            responsiveTables.style.overflow = 'auto';
        });
    }

    document.querySelectorAll(selectorPrefix + ".jj-numeric").forEach(applyDecimalPlaces)

    getMasterDataForm()?.addEventListener("submit", function (event) {
        let isValid: boolean;

        if (typeof jQuery == 'function'){
            isValid = $(this).valid();
        } else if (typeof this.reportValidity === "function") {
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
