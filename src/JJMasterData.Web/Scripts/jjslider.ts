class JJSlider {
    static observeSliders() {
        let sliders = document.getElementsByClassName("jjslider");

        Array.from(sliders).forEach((slider : HTMLInputElement) => {
            let sliderInput = <HTMLInputElement>document.getElementById(slider.id + "-value");

            document.getElementById(slider.id).addEventListener('change', function () {
                this.setAttribute('value', (<HTMLInputElement>(this)).value);
            });

            slider.oninput = function () {
                sliderInput.value = (<HTMLInputElement>(this)).value;
            }
        });
    }

    static observeInputs() {
        let inputs = document.getElementsByClassName("jjslider-value");

        Array.from(inputs).forEach((input: HTMLInputElement) => {
            let slider = <HTMLInputElement>document.getElementById(input.id.replace("-value", ""));

            input.oninput = function () {
                slider.value = (<HTMLInputElement>(this)).value;
            }
        });
    }
}