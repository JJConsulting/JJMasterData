class TooltipListener{
    static listen(selectorPrefix){
        const tooltipTriggerList = document.querySelectorAll(selectorPrefix + '[data-bs-toggle="tooltip"]')
        tooltipTriggerList.forEach(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl,{   trigger : 'hover'}))
    }
}