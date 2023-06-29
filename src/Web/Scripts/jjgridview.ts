class JJGridView {
    static setup() {

    }
    static Sorting(componentName, url, tableOroder) {
        var tableOrder = "#current_tableorder_" + componentName;
        if (tableOroder + " ASC" == $(tableOrder).val())
            $(tableOrder).val(tableOroder + " DESC");
        else
            $(tableOrder).val(tableOroder + " ASC");

        $("#current_tableaction_" + componentName).val("");
        $("#current_formaction_" + componentName).val("");

        JJGridView.RefreshGrid(componentName, url);
    }
    
    static Pagination(componentName, url, currentPage) {
        $("#current_tablepage_" + componentName).val(currentPage);
        $("#current_tableaction_" + componentName).val("");
        $("#current_formaction_" + componentName).val("");

        JJGridView.RefreshGrid(componentName, url);
    }

    static Filter(componentName, url) {
        $("#current_filteraction_" + componentName).val("FILTERACTION");
        $("#current_tableaction_" + componentName).val("");
        $("#current_tablepage_" + componentName).val("1");
        $("#current_formaction_" + componentName).val("");
        JJGridView.RefreshGrid(componentName, url);
    }

    static Refresh(componentName, url) {
        $("#current_tableaction_" + componentName).val("");
        $("#current_tablerow_" + componentName).val("");
        $("#current_formaction_" + componentName).val("");
        JJGridView.RefreshGrid(componentName, url);
    }

    static RefreshGrid(componentName, url) {
        const frm = $("form");

        $.ajax({
            async: true,
            type: frm.attr("method"),
            url: url,
            data: frm.serialize(),
            success: function (data) {
                $("#jjgridview_" + componentName).html(data);
                jjloadform();
                $("#current_filteraction_" + componentName).val("");
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
                console.log(textStatus);
                console.log(jqXHR);
                $("#current_filteraction_" + componentName).val("");
            }
        });
    }
}