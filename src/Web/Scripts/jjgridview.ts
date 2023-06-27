class JJGridView {

    static setup() {

    }

    static Sorting(objid, url, tableOroder) {
        var tableOrder = "#current_tableorder_" + objid;
        if (tableOroder + " ASC" == $(tableOrder).val())
            $(tableOrder).val(tableOroder + " DESC");
        else
            $(tableOrder).val(tableOroder + " ASC");

        $("#current_tableaction_" + objid).val("");
        $("#current_formaction_" + objid).val("");

        JJGridView.RefreshGrid(objid, url);
    }


    static Pagination(objid, url, currentPage) {
        $("#current_tablepage_" + objid).val(currentPage);
        $("#current_tableaction_" + objid).val("");
        $("#current_formaction_" + objid).val("");

        JJGridView.RefreshGrid(objid, url);
    }

    static Filter(objid, url) {
        $("#current_filteraction_" + objid).val("FILTERACTION");
        $("#current_tableaction_" + objid).val("");
        $("#current_tablepage_" + objid).val("1");
        $("#current_formaction_" + objid).val("");
        JJGridView.RefreshGrid(objid, url);
    }

    static Refresh(objid, url) {
        $("#current_tableaction_" + objid).val("");
        $("#current_tablerow_" + objid).val("");
        $("#current_formaction_" + objid).val("");
        JJGridView.RefreshGrid(objid, url);
    }

    static RefreshGrid(objid, url) {
        const frm = $("form");

        $.ajax({
            async: true,
            type: frm.attr("method"),
            url: url,
            data: frm.serialize(),
            success: function (data) {
                $("#jjgridview_" + objid).html(data);
                jjloadform();
                $("#current_filteraction_" + objid).val("");
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
                console.log(textStatus);
                console.log(jqXHR);
                $("#current_filteraction_" + objid).val("");
            }
        });
    }
}