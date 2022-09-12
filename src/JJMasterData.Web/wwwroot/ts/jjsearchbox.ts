class JJSearchBox{
    static setup(){
        $("input.jjsearchbox").each(function () {
            var objid = $(this).attr("jjid");
            var pnlname = $(this).attr("pnlname");
            var triggerlength = $(this).attr("triggerlength");
            var numberofitems = $(this).attr("numberofitems");
            var scrollbar = Boolean($(this).attr("scrollbar"));
            var showimagelegend = Boolean($(this).attr("showimagelegend"));

            if (triggerlength == null)
                triggerlength = "1";

            if (numberofitems == null)
                numberofitems = "10";

            if (scrollbar == null)
                scrollbar = false;

            if (showimagelegend == null)
                showimagelegend = false;

            var frm = $("form");
            var urltypehead = frm.attr("action");
            if (urltypehead.includes("?"))
                urltypehead += "&";
            else
                urltypehead += "?";

            urltypehead += "t=jjsearchbox";
            urltypehead += "&objname=" + objid;
            urltypehead += "&pnlname=" + pnlname;

            $(this).blur(function () {
                if ($(this).val() == "") {
                    $("#st_" + objid)
                        .removeClass("fa-check")
                        .removeClass("fa-exclamation-triangle");
                    $("#" + objid).val("");
                }
            });

            // @ts-ignore
            $(this).typeahead(<any>{
                ajax: {
                    url: urltypehead,
                    method: "POST",
                    loadingClass: "loading-circle",
                    triggerLength: triggerlength,
                    preDispatch: function () {
                        $("#" + objid).val("");
                        $("#st_" + objid)
                            .removeClass("fa-check")
                            .removeClass("fa-exclamation-triangle ");
                        var data = frm.serializeArray();
                        return data;
                    },
                },
                onSelect: function (item) {
                    $("#" + objid).val(item.value);
                    if (item.value != "") {
                        $("#st_" + objid)
                            .removeClass("fa-exclamation-triangle ")
                            .addClass("fa fa-check");
                    }
                },
                displayField: "name",
                valueField: "id",
                triggerLength: triggerlength,
                items: numberofitems,
                scrollBar: scrollbar,
                item: '<li class="dropdown-item"><a href="#"></a></li>',
                highlighter: function (item) {
                    var query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, "\\$&");
                    var textSel;
                    if (showimagelegend) {
                        var parts = item.split("|");
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