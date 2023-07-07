class JJDataPanel{
    static doReload(panelname, objid){
        let url = new UrlBuilder()
        url.addQueryParameter("pnlname",panelname)
        url.addQueryParameter("objname",objid)
        
        DataPanel.Reload(url.build(), panelname, objid)
    }
}