class OffcanvasHelper{
    
    static showOffcanvas(id){
        const offcanvasElement = bootstrap.Offcanvas.getOrCreateInstance(document.getElementById(id));
        offcanvasElement.show();
    }
    
    static async populateOffcanvas(id, url) {
        const offcanvasElement = bootstrap.Offcanvas.getOrCreateInstance(document.getElementById(id));
        const response = await fetch(url);
        const data = await response.text();
        const offcanvasBody = document.getElementById(`${id}-body`);
        offcanvasBody.innerHTML = data;
        offcanvasElement.show();
    }
}