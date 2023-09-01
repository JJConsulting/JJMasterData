class SearchBoxListener {
    static listenTypeahed(){
        $("input.jj-search-box").each(function () {
            const hiddenInputId = $(this).attr("hidden-input-id");
            let urltypehead: string = $(this).attr("urltypehead");
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

            const jjSearchBoxSelector = "#" + hiddenInputId + "_text";
            const jjSearchBoxHiddenSelector = "#" + hiddenInputId;
            
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
                    const hiddenSearchBox = document.querySelector<HTMLInputElement>(jjSearchBoxHiddenSelector);
                    
                    if(hiddenSearchBox)
                        hiddenSearchBox.value = item.value;
                    
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