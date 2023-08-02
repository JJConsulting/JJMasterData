class FeedbackIcon {
    public static searchClass = "jj-icon-search";
    public static successClass = "jj-icon-success";
    public static warningClass = "jj-icon-warning";
    public static errorClass = "jj-icon-error";

    public static removeAllIcons(selector: string){
        $(selector)
            .removeClass(FeedbackIcon.successClass)
            .removeClass(FeedbackIcon.warningClass)
            .removeClass(FeedbackIcon.searchClass)
            .removeClass(FeedbackIcon.errorClass)
    }
    
    public static setIcon(selector: string, iconClass : string){
        this.removeAllIcons(selector)
        $(selector).addClass(iconClass);
    }
}