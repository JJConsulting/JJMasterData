class SliderListener {
    static listenSliders(selectorPrefix = String()) {
        let sliders = document.querySelectorAll(selectorPrefix + ".jjslider");

        Array.from(sliders).forEach((slider : HTMLInputElement) => {
            const sliderInput = <HTMLInputElement>document.getElementById(slider.id + "-value");

            document.getElementById(slider.id).addEventListener('change', function () {
                this.setAttribute('value', (<HTMLInputElement>(this)).value);
            });

            slider.oninput = function () {
                sliderInput.value = (<HTMLInputElement>(this)).value;
            }
        });
    }

    static listenInputs(selectorPrefix = String()) {
        let inputs = document.querySelectorAll(selectorPrefix + ".jjslider-value");

        Array.from(inputs).forEach((input: HTMLInputElement) => {
            let slider= <HTMLInputElement>document.getElementById(input.id.replace("-value", ""));

            input.oninput = function () {
                // @ts-ignore
                slider.value = document.getElementById(input.id).value;
            }
        });
    }
}