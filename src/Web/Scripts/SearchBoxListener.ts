class SearchBoxListener {
    static listenTypeahead(selectorPrefix = String()) {
        $(selectorPrefix + "input.jj-search-box").each(function () {
            const hiddenInputId = $(this).attr("hidden-input-id");
            let queryString: string = $(this).attr("query-string");
            let triggerLength = $(this).attr("trigger-length");
            let numberOfItems = $(this).attr("number-of-items");

            if (triggerLength == null)
                triggerLength = "3";

            if (numberOfItems == null)
                numberOfItems = "10";
            
            const urlBuilder = new UrlBuilder();
            for (const pair of queryString.split("&")) {
                const [key, value] = pair.split("=");
                if (key && value) {
                    urlBuilder.addQueryParameter(key, value);
                }
            }

            const url = urlBuilder.build();

            const jjSearchBoxSelector = "#" + hiddenInputId + "_text";
            const jjSearchBoxHiddenSelector = "#" + hiddenInputId;
            
            $(this).on("blur",function () {
                if ($(this).val() == "") {
                    FeedbackIcon.setIcon(jjSearchBoxSelector, FeedbackIcon.searchClass)
                    $(jjSearchBoxHiddenSelector).val("");
                }
                else if($(jjSearchBoxHiddenSelector).val() == ""){
                    FeedbackIcon.setIcon(jjSearchBoxSelector, FeedbackIcon.warningClass)
                }
                else{
                    FeedbackIcon.setIcon(jjSearchBoxSelector, FeedbackIcon.successClass)
                }
                $(jjSearchBoxSelector).prev().attr("style","display:none")
                $(jjSearchBoxSelector).css("background-color", '');
            });
            
            $(this).typeahead({
                    hint: true,
                    highlight: true,
                    autoselect: true,
                    minLength: triggerLength,
                    limit: numberOfItems,
                    classNames: {
                        dataset: "list-group",
                        cursor: "active",
                        menu: "search-box-menu"
                    }
                },
                {
                    displayKey: "description",
                    source: function (query, syncResults, asyncResults) {
                        if(query.length == 0)
                            return;
                        
                        FeedbackIcon.removeAllIcons(jjSearchBoxSelector);
                        $(jjSearchBoxSelector).addClass("loading-circle");
                        fetch(url, getRequestOptions())
                            .then(response => response.json())
                            .then(data => {
                                $(jjSearchBoxSelector).removeClass("loading-circle");
                                asyncResults(data);
                            })
                            .catch(error => {
                                console.error(error);
                                $(jjSearchBoxSelector).removeClass("loading-circle");
                                FeedbackIcon.setIcon(jjSearchBoxSelector, FeedbackIcon.errorClass)
                            })
                    },
                    templates: {
                        suggestion: function (value) {
                            if(value.icon){
                                return `<div class="list-group-item"><span class="fa ${value.icon}" style="color:${value.iconColor}"></span>&nbsp;${value.description}</div>`
                            }
                            return `<div class="list-group-item">${value.description}</div>`
                        }
                    }
                });

            $(this).bind('typeahead:select', function (ev, selectedValue) {
                const hiddenSearchBox = document.querySelector<HTMLInputElement>(jjSearchBoxHiddenSelector);

                if (hiddenSearchBox)
                    hiddenSearchBox.value = selectedValue.id;

                if (selectedValue.id != "") {
                    FeedbackIcon.setIcon(jjSearchBoxSelector, FeedbackIcon.successClass)
                }
            });

            $('.typeahead').bind('typeahead:change', function(ev, suggestion) {
                if ($(jjSearchBoxHiddenSelector).val() == "") {
                    FeedbackIcon.setIcon(jjSearchBoxSelector, FeedbackIcon.warningClass)
                }
            });
            $(jjSearchBoxSelector).prev().attr("style","display:none")
            $(jjSearchBoxSelector).css("background-color", '');
            $(jjSearchBoxSelector).closest('.twitter-typeahead').css('display', '');
        });
    }
}