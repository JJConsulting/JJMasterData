document.addEventListener("DOMContentLoaded", function () {
    const masterDataScrollPosition = localStorage.getItem("masterDataScrollPosition");
    
    if(masterDataScrollPosition){
        window.scrollTo({
            top: Number.parseFloat(masterDataScrollPosition),
            //@ts-ignore
            behavior: "instant"
        });
        localStorage.removeItem("masterDataScrollPosition");
    }

    Localization.initialize();
    
    listenAllEvents()
});