const listenAllEvents = (selectorPrefix: string = String()) => {
    selectorPrefix += " "
    
    TomSelectHelper.listen(selectorPrefix);
    
    CodeEditor.setup(selectorPrefix);
    CalendarListener.listen(selectorPrefix);
    PhoneInputListener.listen(selectorPrefix);
    TextAreaListener.listenKeydown(selectorPrefix);
    
    SearchBoxListener.listen(selectorPrefix);
    
    LookupListener.listenChanges(selectorPrefix);
    SortableListener.listenSorting(selectorPrefix);
    UploadAreaListener.listenFileUpload(selectorPrefix);
    TabNavListener.listenTabNavs(selectorPrefix);
    SliderListener.listenSliders(selectorPrefix);
    SliderListener.listenInputs(selectorPrefix);
    HierarchyListener.listen(selectorPrefix);
    
    GridViewHelper.hideEmptyColumns(selectorPrefix);
    GridViewHelper.setupInfiniteScroll();
    
    //@ts-ignore
    Inputmask().mask(document.querySelectorAll(selectorPrefix+"input"));
    
    TooltipHelper.listen(selectorPrefix);
    
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
