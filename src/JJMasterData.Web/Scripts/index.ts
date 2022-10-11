document.addEventListener("DOMContentLoaded", function() {
    // @ts-ignore
    bootstrapVersion = $.fn.tooltip.Constructor.VERSION.charAt(0)
    
    $.ajaxSetup({
        xhrFields: {
            withCredentials: true
        }
    });

    jjloadform("load");
});