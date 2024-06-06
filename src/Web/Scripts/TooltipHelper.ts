class TooltipHelper {
    static dispose(selectorPrefix){
        const tooltipTriggerList = [].slice.call(document.querySelectorAll(selectorPrefix + ' [data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            const bootstrapTooltip = bootstrap.Tooltip.getOrCreateInstance(tooltipTriggerEl);
            bootstrapTooltip.dispose();
        });
    }
    
    static listen(selectorPrefix){
        const tooltipTriggerList = document.querySelectorAll(selectorPrefix + ' [data-bs-toggle="tooltip"]')
        tooltipTriggerList.forEach(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl,{   trigger : 'hover'}))
    }
    
    static createTooltip(id){
        document.addEventListener("DOMContentLoaded", function (){
            new bootstrap.Tooltip(document.getElementById(id),{ trigger : 'hover'})
        });
    }
}