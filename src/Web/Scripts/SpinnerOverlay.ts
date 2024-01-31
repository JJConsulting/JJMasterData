class SpinnerOverlay {
    private static spinnerOverlayId = "spinner-overlay";
    private static spinner;
    public static visible = true;
    private static loadHtml(): void {
        if (!document.querySelector("#"+this.spinnerOverlayId)) {

            if (bootstrapVersion < 5) {
                const spinnerOverlay = document.createElement("div");
                spinnerOverlay.id = this.spinnerOverlayId;
                spinnerOverlay.innerHTML = `
            <div class="ajaxImage"></div>
            <div class="ajaxMessage">Loading...</div>
            `;
                document.body.appendChild(spinnerOverlay);

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
                //@ts-ignore
                const spinner = new Spinner(options).spin();
                if (spinner.el) {
                    const spinnerOverlayElement = document.querySelector("#spinner-overlay .ajaxImage");
                    spinnerOverlayElement.parentNode.insertBefore(spinner.el, spinnerOverlayElement.nextSibling);
                }
            } else {
                const spinnerOverlayDiv = document.createElement('div');
                spinnerOverlayDiv.id = this.spinnerOverlayId;
                spinnerOverlayDiv.classList.add('spinner-overlay', 'text-center');
                const spinnerDiv = document.createElement('div');
                spinnerDiv.classList.add('spinner-border','spinner-border-lg');
                spinnerDiv.setAttribute('role', 'status');
                const spanElement = document.createElement('span');
                spanElement.classList.add('visually-hidden');
                spanElement.textContent = 'Loading...';
                spinnerDiv.appendChild(spanElement);
                spinnerOverlayDiv.appendChild(spinnerDiv);
                document.body.appendChild(spinnerOverlayDiv);
            }
        }

    }

    public static show(): void {
        if(this.visible){
            this.loadHtml();
            document.querySelector<HTMLElement>("#" + this.spinnerOverlayId).style.display = "";
        }
    }

    public static hide(): void {
        if(this.visible){
            const overlay = document.querySelector<HTMLElement>("#" + this.spinnerOverlayId);

            if (overlay) {
                overlay.style.display = "none";
            }
        }
    }
}