class CollapsePanelListener{
    static listen(name) {
        let nameSelector = "#" + name;
        let collapseSelector = '#collapse_mode_' + name;

        document.addEventListener("DOMContentLoaded", function() {
            let collapseElement = document.querySelector(nameSelector);

            collapseElement.addEventListener("hidden.bs.collapse", function() {
                document.querySelector<HTMLInputElement>(collapseSelector).value = "0";
            });

            collapseElement.addEventListener("show.bs.collapse", function() {
                document.querySelector<HTMLInputElement>(collapseSelector).value = "1";
            });
        });
    }
}