class OffcanvasHelper{
    
    static showOffcanvas(id: string, url: string = null){
        const offcanvasElement = bootstrap.Offcanvas.getOrCreateInstance(document.getElementById(id));
        
        if(url != null){
            const offcanvasBody = document.getElementById(`${id}-body`);
            if(offcanvasBody.childElementCount === 0){
                fetch(url).then(response=>{
                   return response.text();
                }).then(data=>{
                    HTMLHelper.setInnerHTML(offcanvasBody, data);
                });
            }
        }        
        
        offcanvasElement.show();
    }
    static hide(id){
        const offcanvasElement = bootstrap.Offcanvas.getOrCreateInstance(document.getElementById(id));
        offcanvasElement.hide();
    }
}