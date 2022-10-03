function setupCollapsePanel(name : string){
    let nameSelector = "#" + name
    let collapseSelector = '#collapse_mode_' + name
    
    document.addEventListener("DOMContentLoaded", function() {
        $(nameSelector).on('hidden.bs.collapse', function () {
            $(collapseSelector).val("0");
        });

        $(nameSelector).on('show.bs.collapse', function () {
            $(collapseSelector).val("1");
        });
    })
}