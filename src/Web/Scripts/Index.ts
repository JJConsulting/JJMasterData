document.addEventListener("DOMContentLoaded", function () {
    const masterDataScrollPosition = localStorage.getItem("masterDataScrollPosition");
    if(masterDataScrollPosition){
        window.scroll(0,Number.parseFloat(masterDataScrollPosition));
        localStorage.removeItem("masterDataScrollPosition");
    }
    listenAllEvents()
});