class SliderListener {
    static listenSliders() {
        let sliders = document.getElementsByClassName("jjslider");

        Array.from(sliders).forEach((slider : HTMLInputElement) => {
            let sliderInput = <HTMLInputElement>document.getElementById(slider.id + "-value");

            document.getElementById(slider.id).addEventListener('change', function () {
                this.setAttribute('value', (<HTMLInputElement>(this)).value);
            });

            slider.oninput = function () {
                let decimalPlaces = $(this).attr("jjdecimalplaces");
                if (decimalPlaces == null)
                    decimalPlaces = "0";

                let sliderValue = (<HTMLInputElement>(this)).value;
                
                if(localeCode==='pt')
                    // @ts-ignore
                    sliderInput.value = $.number(sliderValue, decimalPlaces, ",", ".");
                else
                    // @ts-ignore
                    sliderInput.value = $.number(sliderValue, decimalPlaces);
            }
        });
    }

    static listenInputs() {
        let inputs = document.getElementsByClassName("jjslider-value");

        Array.from(inputs).forEach((input: HTMLInputElement) => {
            let slider= <HTMLInputElement>document.getElementById(input.id.replace("-value", ""));

            input.oninput = function () {
                // @ts-ignore
                slider.value = $("#" + input.id).val();
            }
        });
    }
}