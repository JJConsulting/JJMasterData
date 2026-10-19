class SpinnerOverlay {
    private static spinnerOverlayId = "spinner-overlay";
    private static spinner;
    public static visible = true;
    private static loadHtml(): void {
        if (!document.querySelector("#"+this.spinnerOverlayId)) {

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
