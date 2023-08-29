class UploadView{
    static open(componentName: string, title:string, values: string, url: string = null){

        const panelName = $("#v_" + componentName).attr("panelName");
        
        if(url == null){
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("jjuploadview_" + panelName, componentName)
            urlBuilder.addQueryParameter("uploadViewParams",values)
            
            url = urlBuilder.build();
        }
        
        const modal = new Modal();
        modal.modalId =componentName + "-upload-popup"
        modal.modalTitleId = componentName + "-upload-popup-title"

        if(url == null || url.length == 0){
            modal.show(title, url , 1);
        }
        else{
            modal.showHtmlFromUrl(title, url,null, 1).then(_=>{
                loadJJMasterData()
            })
        }

    }
}