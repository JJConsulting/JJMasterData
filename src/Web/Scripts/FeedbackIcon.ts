class FeedbackIcon {
    public static searchClass = "jj-icon-search";
    public static successClass = "jj-icon-success";
    public static warningClass = "jj-icon-warning";
    public static errorClass = "jj-icon-error";

    public static removeAllIcons(selector: string) {
        const elements = this.findElements(selector);

        elements?.forEach(element => {
            element.classList.remove(
                FeedbackIcon.successClass,
                FeedbackIcon.warningClass,
                FeedbackIcon.searchClass,
                FeedbackIcon.errorClass
            );
        });
    }

    public static setIcon(selector: string, iconClass: string) {
        this.removeAllIcons(selector);

        const elements = this.findElements(selector);

        elements?.forEach(element => {
            element.classList.add(iconClass);
        });
    }

    private static findElements(selector: string) {
        let elements = document.querySelectorAll(selector);

        if (elements.length === 0 && window.parent !== window) {
            elements = window.parent.document.querySelectorAll(selector);
        }
        return elements;
    }

}
