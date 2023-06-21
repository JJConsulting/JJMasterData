class JJSearchBox{
    static setup(){
        $("input.jjsearchbox").each(function () {
            const objid = $(this).attr("jjid");
            const fieldName = $(this).attr("fieldName");
            const dictionaryName = $(this).attr("dictionaryName");
            const pageState = $(this).attr("pageState");
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
            urltypehead += "&fieldName=" + fieldName;
            urltypehead += "&objname=" + $(this).attr("name");
            urltypehead += "&dictionaryName=" + dictionaryName;
            urltypehead += "&pageState=" + pageState;
            
            const jjSearchBoxSelector = "#" + objid + "_text";
            const jjSearchBoxHiddenSelector = "#" + objid;
            
            $(this).blur(function () {
                if ($(this).val() == "") {
                    JJFeedbackIcon.setIcon(jjSearchBoxSelector, JJFeedbackIcon.searchClass)
                    $(jjSearchBoxHiddenSelector).val("");
                }
                else if($(jjSearchBoxHiddenSelector).val() == ""){
                    JJFeedbackIcon.setIcon(jjSearchBoxSelector, JJFeedbackIcon.warningClass)
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
                        JJFeedbackIcon.setIcon(jjSearchBoxSelector, "")
                        
                        return frm.serializeArray();
                    },
                },
                onSelect: function (item) {
                    $(jjSearchBoxHiddenSelector).val(item.value);
                    if (item.value != "") {
                        JJFeedbackIcon.setIcon(jjSearchBoxSelector, JJFeedbackIcon.successClass)
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