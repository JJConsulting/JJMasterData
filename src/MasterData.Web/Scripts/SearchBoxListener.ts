class SearchBoxListener {
    static listen(selectorPrefix = "") {
        const inputs = document.querySelectorAll<HTMLInputElement>(`${selectorPrefix}input.jj-search-box`);

        inputs.forEach(input => {
            const hiddenInputId = input.getAttribute("hidden-input-id");
            let queryString = input.getAttribute("query-string") || "";
            let triggerLength = Number(input.getAttribute("trigger-length") || "1");
            let numberOfItems = Number(input.getAttribute("number-of-items") || "30");
            let multiSelect = Number(input.getAttribute("multiselect")) == 1;
            
            const urlBuilder = new UrlBuilder();
            
            queryString.split("&").forEach(pair => {
                const [key, value] = pair.split("=");
                if (key && value)
                    urlBuilder.addQueryParameter(key, value);
            });
            
            const url = urlBuilder.build();

            // @ts-ignore
            new BootstrapSearch(input, {
                remoteData: q => url + "&q=" + q,
                inputLabel: "description",
                dropdownLabel: function(value){
                    if(value.icon){
                        return `<div><span class="fa ${value.icon}" style="color:${value.iconColor}"></span>&nbsp;${value.description}</div>`
                    }
                    if(value.imageUrl){
                        return `<div class="search-box-image-label"><img src="${value.imageUrl}" alt="result"/>${value.description}</div>`;
                    }
                    return `<div>${value.description}</div>`
                },
                value: "id",
                threshold: triggerLength,
                maximumItems: numberOfItems,
                multiSelect: multiSelect,
                onSelectItem: selected => {
                    if (!hiddenInputId) 
                        return;
                    const hiddenInput = document.getElementById(hiddenInputId) as HTMLInputElement;
                    if (!hiddenInput)
                        return;

                    if (Array.isArray(selected)) {
                        hiddenInput.value = selected.map(s => s.value).join(",");
                    } else if (selected) {
                        hiddenInput.value = selected.value;
                    } else {
                        hiddenInput.value = "";
                    }
                }
            });
        });
    }
}
