class UploadView{
    static open(componentName: string, title:string, values: string, url: string = null){

        const panelName = $("#v_" + componentName).attr("panelName");
        
        if(url == null){
            const urlBuilder = new UrlBuilder();
            urlBuilder.addQueryParameter("jjuploadview_" + panelName, componentName)
            urlBuilder.addQueryParameter("uploadViewParams",values)
            
            url = urlBuilder.build();
        }
        
        const popup = new Modal();
        popup.modalId =componentName + "-popup"
        popup.modalTitleId = componentName + "-popup-title"

        if(url == null || url.length == 0){
            popup.show(title, url , 1);
        }
        else{
            popup.showHtmlFromUrl(title, url,null, 1).then(_=>{
                loadJJMasterData()
            })
        }

    }
}