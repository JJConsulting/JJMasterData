class SearchBoxListener {
    static listenTypeahed(selectorPrefix = String()){
        $(selectorPrefix + "input.jj-search-box").each(function () {
            const hiddenInputId = $(this).attr("hidden-input-id");
            let queryString: string = $(this).attr("query-string");
            let triggerLength = $(this).attr("trigger-length");
            let numberOfItems = $(this).attr("number-of-items");
            let scrollbar = Boolean($(this).attr("scrollbar"));
            let showImageLegend = Boolean($(this).attr("show-image-legend"));

            if (triggerLength == null)
                triggerLength = "1";

            if (numberOfItems == null)
                numberOfItems = "10";

            if (scrollbar == null)
                scrollbar = false;
        
            if (showImageLegend == null)
                showImageLegend = false;

            const form = $("form");
            
            let url = new UrlBuilder().build();
            
            if(!url.endsWith("?"))
                url += "?";
            
            url += queryString;
            
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
                    url: url,
                    method: "POST",
                    loadingClass: "loading-circle",
                    triggerLength: triggerLength,
                    preDispatch: function () {
                        $(jjSearchBoxHiddenSelector).val("");
                        FeedbackIcon.removeAllIcons(jjSearchBoxSelector)
                        return form.serializeArray();
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
                triggerLength: triggerLength,
                items: numberOfItems,
                scrollBar: scrollbar,
                item: '<li class="dropdown-item"><a href="#"></a></li>',
                highlighter: function (item) {
                    const query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, "\\$&");
                    let textSel;
                    if (showImageLegend) {
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