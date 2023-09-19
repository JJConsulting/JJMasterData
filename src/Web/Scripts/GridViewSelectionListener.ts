class GridViewSelectionListener {
    private static listenGridViewCheckboxes(componentName: string) {
        document.querySelectorAll(`#${componentName} .jj-checkbox input`)
            .forEach((input: HTMLInputElement) => this.listenCheckboxChange(componentName, input));
    }

    private static listenCheckboxChange(componentName: string, input: HTMLInputElement) {
        input.addEventListener('change', function () {
            GridViewSelectionHelper.selectItem(componentName, input);
        }, {once: true});
    }

    static listen(componentName: string) {
        document.addEventListener('DOMContentLoaded', function () {
            GridViewSelectionListener.listenGridViewCheckboxes(componentName);

            const observer = new MutationObserver(() => GridViewSelectionListener.listenGridViewCheckboxes(componentName));
            observer.observe(document.getElementById("grid-view-" + componentName), {
                attributes: false,
                childList: true,
                subtree: true
            });
        });
    }
}