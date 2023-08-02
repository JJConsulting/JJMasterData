class SearchBox {
    static setup(){
        $("input.jjsearchbox").each(function () {
            const componentName = $(this).attr("jjid");
            let urltypehead = $(this).attr("urltypehead");
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

            if (!urltypehead.includes("GetItems")) {
                let url = frm.attr("action");
                if (url.includes("?"))
                    url += "&";
                else
                    url += "?";

                urltypehead = url + urltypehead;
            }

            const jjSearchBoxSelector = "#" + componentName + "_text";
            const jjSearchBoxHiddenSelector = "#" + componentName;
            
            $(this).blur(function () {
                if ($(this).val() == "") {
                    FeedbackIcon.setIcon(jjSearchBoxSelector, FeedbackIcon.searchClass)
                    $(jjSearchBoxHiddenSelector).val("");
                }
                else if($(jjSearchBoxHiddenSelector).val() == ""){
                    FeedbackIcon.setIcon(jjSearchBoxSelector, FeedbackIcon.warningClass)
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
                        FeedbackIcon.setIcon(jjSearchBoxSelector, "")
                        
                        return frm.serializeArray();
                    },
                },
                onSelect: function (item) {
                    $(jjSearchBoxHiddenSelector).val(item.value);
                    if (item.value != "") {
                        FeedbackIcon.setIcon(jjSearchBoxSelector, FeedbackIcon.successClass)
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