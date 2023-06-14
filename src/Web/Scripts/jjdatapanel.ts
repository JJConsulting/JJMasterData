class JJDataPanel{
    static doReload(panelname, objid){
        const frm = $("form");
        let surl = frm.attr("action");
        surl += surl.includes("?") ? "&" : "?";
        surl += "t=reloadpainel";
        surl += "&pnlname=" + panelname;
        surl += "&objname=" + objid;
        $.ajax({
            async: true,
            type: frm.attr("method"),
            url: surl,
            data: frm.serialize(),
            success: function (data) {
                $("#" + panelname).html(data);
                jjloadform();
                jjutil.gotoNextFocus(objid);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
                console.log(textStatus);
                console.log(jqXHR);
            }
        });
    }
}