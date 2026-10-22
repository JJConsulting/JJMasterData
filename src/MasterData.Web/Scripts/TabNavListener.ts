class TabNavListener{
    static listenTabNavs(selectorPrefix = String()){
        $(selectorPrefix + ".jj-tab-link").on("shown.bs.tab", function (e) {
            const tab = e.target as HTMLElement;
            const inputId = tab.dataset.jjObjectId;
            const selectedTabIndex = tab.dataset.jjTabIndex;
            const selectedTabInput = inputId ? document.getElementById(inputId) : null;

            if (selectedTabInput instanceof HTMLInputElement && selectedTabIndex !== undefined) {
                selectedTabInput.value = selectedTabIndex;
            }
        });
    }
}
