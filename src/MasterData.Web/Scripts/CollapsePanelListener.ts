class CollapsePanelListener{
    static listen(componentName: string) {
        let nameSelector = "#" + componentName;
        let collapseSelector = '#'+componentName + '-is-open';

        let collapseElement = document.querySelector(nameSelector);

        if(bootstrapVersion === 5){
            collapseElement.addEventListener("hidden.bs.collapse", function () {
                document.querySelector<HTMLInputElement>(collapseSelector).value = "0";
            });
            collapseElement.addEventListener("show.bs.collapse", function () {
                document.querySelector<HTMLInputElement>(collapseSelector).value = "1";
            });
        }
        else{
            $(nameSelector).on('hidden.bs.collapse', function () {
                $(collapseSelector).val("0");
            });

            $(nameSelector).on('show.bs.collapse', function () {
                $(collapseSelector).val("1");
            });
        }
    }
}