class SpinnerOverlay {
    private static spinnerOverlayId = "#spinner-overlay";
    private static spinner;

    private static loadHtml(): void {
        if (!$(this.spinnerOverlayId).length) {
            const html = `
                <div id="spinner-overlay">
                    <div class="ajaxImage"></div>
                    <div class="ajaxMessage">Loading...</div>
                </div>
            `;
            $(html).insertAfter($("body"));

            const options = {
                lines: 17,
                length: 28,
                width: 14,
                radius: 38,
                scale: 0.40,
                corners: 1,
                color: "#000",
                opacity: 0.3,
                rotate: 0,
                direction: 1,
                speed: 1.2,
                trail: 62,
                fps: 20,
                zIndex: 2e9,
                className: "spinner",
                top: "50%",
                left: "50%",
                shadow: false,
                hwaccel: false,
                position: "absolute",
            };

            // @ts-ignore
            this.spinner = new Spinner(options).spin();

            if (this.spinner.el) {
                $(this.spinner.el).insertAfter($("#spinner-overlay .ajaxImage"));
            }
        }
    }

    public static show(): void {
        this.loadHtml();
        document.querySelector<HTMLElement>(this.spinnerOverlayId).style.display = "";
    }
    
    public static hide(): void {
        document.querySelector<HTMLElement>(this.spinnerOverlayId).style.display = "none";
    }
}