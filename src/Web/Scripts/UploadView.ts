class UploadView{
    static open(componentName: string, title:string, values: string, url: string = null){

        const panelName = $("#v_" + componentName).attr("panelName");
        
        if(url == null || url.length == 0){
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("uploadView-" + panelName, componentName)
            urlBuilder.addQueryParameter("uploadViewParams",values)
            url = urlBuilder.build();
        }
        
        const modal = new Modal();
        modal.modalId =componentName + "-upload-popup"
        modal.modalTitleId = componentName + "-upload-popup-title"
        
        modal.showHtmlFromUrl(title, url,null, 1).then(_=>{
            loadJJMasterData()
        })

    }
}