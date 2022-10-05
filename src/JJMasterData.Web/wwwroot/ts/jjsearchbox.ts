class JJSearchBox{
    static setup(){
        $("input.jjsearchbox").each(function () {
            const objid = $(this).attr("jjid");
            const pnlname = $(this).attr("pnlname");
            
            let triggerlength = $(this).attr("triggerlength");
            let numberofitems = $(this).attr("numberofitems");
            let scrollbar = Boolean($(this).attr("scrollbar"));
            let showimagelegend = Boolean($(this).attr("showimagelegend"));

            if (triggerlength == null)
                triggerlength = "1";

            if (numberofitems == null)
                numberofitems = "10";

            if (scrollbar == null)
                scrollbar = false;

            if (showimagelegend == null)
                showimagelegend = false;

            const frm = $("form");
            
            let urltypehead = frm.attr("action");
            if (urltypehead.includes("?"))
                urltypehead += "&";
            else
                urltypehead += "?";

            urltypehead += "t=jjsearchbox";
            urltypehead += "&objname=" + objid;
            urltypehead += "&pnlname=" + pnlname;
            
            const jjSearchBoxSelector = "#" + objid + "_text";
            
            const searchClass = "jj-icon-search"
            const successClass = "jj-icon-success"
            const warningClass = "jj-icon-warning"
            
            $(this).blur(function () {
                if ($(this).val() == "") {
                    $(jjSearchBoxSelector)
                        .removeClass(successClass)
                        .removeClass(warningClass);
                    $("#" + objid).val("");
                }
            });
            
            $(this).typeahead({
                ajax: {
                    url: urltypehead,
                    method: "POST",
                    loadingClass: "loading-circle",
                    triggerLength: triggerlength,
                    preDispatch: function () {
                        $("#" + objid).val("");
                        $(jjSearchBoxSelector)
                            .removeClass(successClass)
                            .removeClass(warningClass);
                        const data = frm.serializeArray();
                        return data;
                    },
                },
                onSelect: function (item) {
                    $("#" + objid).val(item.value);
                    if (item.value != "") {
                        $(jjSearchBoxSelector)
                            .removeClass(warningClass)
                            .addClass(successClass);
                    }
                },
                displayField: "name",
                valueField: "id",
                triggerLength: triggerlength,
                items: numberofitems,
                scrollBar: scrollbar,
                item: '<li class="dropdown-item"><a href="#"></a></li>',
                highlighter: function (item) {
                    const query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, "\\$&");
                    let textSel;
                    if (showimagelegend) {
                        const parts = item.split("|");
                        textSel = parts[0].replace(new RegExp("(" + query + ")", "ig"), function ($1, match) {
                            return "<strong>" + match + "</strong>";
                        });
                        textSel = "<i class='fa fa-lg fa-fw " + parts[1] + "' style='color:" + parts[2] + ";margin-right:6px;'></i>" + textSel;
                    } else {
                        textSel = item.replace(new RegExp("(" + query + ")", "ig"), function ($1, match) {
                            return "<strong>" + match + "</strong>";
                        });
                    }
                    return textSel;
                }
            });
        });
    }
}