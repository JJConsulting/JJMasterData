class JJSearchBox{

    private static searchClass = "jj-icon-search";
    private static successClass = "jj-icon-success";
    private static warningClass = "jj-icon-warning";
    
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
            const jjSearchBoxHiddenSelector = "#" + objid;
            
            $(this).blur(function () {
                if ($(this).val() == "") {
                    JJSearchBox.setIcon(jjSearchBoxSelector, JJSearchBox.searchClass)
                    $(jjSearchBoxHiddenSelector).val("");
                }
                else if($(jjSearchBoxHiddenSelector).val() == ""){
                    JJSearchBox.setIcon(jjSearchBoxSelector, JJSearchBox.warningClass)
                }
            });
            
            $(this).typeahead({
                ajax: {
                    url: urltypehead,
                    method: "POST",
                    loadingClass: "loading-circle",
                    triggerLength: triggerlength,
                    preDispatch: function () {
                        $(jjSearchBoxHiddenSelector).val("");
                        JJSearchBox.setIcon(jjSearchBoxSelector, "")
                        
                        return frm.serializeArray();
                    },
                },
                onSelect: function (item) {
                    $(jjSearchBoxHiddenSelector).val(item.value);
                    if (item.value != "") {
                        JJSearchBox.setIcon(jjSearchBoxSelector, JJSearchBox.successClass)
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
    
    private static setIcon(selector: string, iconClass : string){
        $(selector)
            .removeClass(JJSearchBox.successClass)
            .removeClass(JJSearchBox.warningClass)
            .removeClass(JJSearchBox.searchClass)
            .addClass(iconClass);
    }
}