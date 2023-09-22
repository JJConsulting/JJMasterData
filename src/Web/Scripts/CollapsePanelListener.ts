class CollapsePanelListener{
    static listen(componentName: string) {
        let nameSelector = "#" + componentName;
        let collapseSelector = '#'+componentName + '-is-open';

        let collapseElement = document.querySelector(nameSelector);

        collapseElement.addEventListener("hidden.bs.collapse", function() {
            document.querySelector<HTMLInputElement>(collapseSelector).value = "0";
        });

        collapseElement.addEventListener("show.bs.collapse", function() {
            document.querySelector<HTMLInputElement>(collapseSelector).value = "1";
        });
    }
}