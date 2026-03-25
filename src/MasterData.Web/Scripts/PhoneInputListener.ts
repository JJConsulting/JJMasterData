class PhoneInputListener {
    static listen(selectorPrefix = "") {
        const selects = document.querySelectorAll<HTMLSelectElement>(`${selectorPrefix}select.jj-phone-select`);
        selects.forEach(select => 
        {
            $(select).on('changed.bs.select', function (e, clickedIndex, isSelected, previousValue) 
            {
                const options = [...select.options];
                const previousOption = options.find(option => option.value === previousValue);
                const previousDialCode = previousOption.getAttribute('dial-code');
                if(isSelected){
                    const selectedOption = options[clickedIndex];
                    const selectedDialCode = selectedOption.getAttribute('dial-code');
                    const input = $(select).closest('.input-group').find('.jj-phone-input')[0];
                    //@ts-ignore
                    if (input && input.inputmask) {
                        //@ts-ignore
                        input.inputmask.remove();
                    }
                    
                    $(input).val($(input).val().toString().replace(previousDialCode.replace(/\s/g, ''), selectedDialCode.replace(/\s/g, '')));
                    //@ts-ignore
                    Inputmask({
                        mask: `+${'9'.repeat(selectedDialCode.replace('+', '').length)}999999[9]9999`,
                        greedy: false,
                        placeholder: " ",
                        autoUnmask: false
                    }).mask(input);
                }
            });
        })

        const inputs = document.querySelectorAll<HTMLInputElement>(`${selectorPrefix}input.jj-phone-input`);
        inputs.forEach(input =>
        {
            const select = $(input).closest('.input-group').find('select.jj-phone-select')[0] as HTMLSelectElement;
            if (!select) return;
            
            const options = [...select.options];
            const longestDialCode = options.reduce((longest, current) => {
                const dialCode = current.getAttribute('dial-code');
                return dialCode.length > longest ? dialCode.length : longest;
            }, 0);

            function getCountryFromInputValue(currentValue:string){
                let countryFound: HTMLOptionElement | undefined;
                let verifySubStrLen = longestDialCode;
                while (!countryFound && verifySubStrLen > 0) {
                    const val = currentValue.substring(0, verifySubStrLen);
                    countryFound = options.find(option => {
                        const dialCode = option.getAttribute('dial-code') || '';
                        return val == dialCode.replace(/\s/g, '');
                    });
                    verifySubStrLen--;
                }
                return countryFound;
            }

            if(input.value.startsWith('+'))
            {
                const optionCountrySelected = getCountryFromInputValue(input.value);
                if(optionCountrySelected)
                    $(select).selectpicker('val', optionCountrySelected.value);
            }
            else
            {
                const optionCountrySelected = $(select).val();
                const optionSelected = options.find(option => option.value === optionCountrySelected);
                $(input).val(optionSelected.getAttribute('dial-code'));
            }
            
            $(input).on('keyup', function (e)
            {
                const currentValue = (this as HTMLInputElement).value;
                let countryFound: HTMLOptionElement | undefined;
                countryFound = getCountryFromInputValue(currentValue);
                if(countryFound)
                    $(select).selectpicker('val', countryFound.value);
                else
                    if(input.value.length >= longestDialCode)
                        $(input).val("+");
            });
        })
    }
}
