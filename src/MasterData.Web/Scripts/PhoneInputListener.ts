class PhoneInputListener {
    static listen(selectorPrefix = "") {
        function normalizeDialCode(dialCode: string | null | undefined) {
            return (dialCode || '').replace(/\s/g, '');
        }

        function getLocalPhoneValue(phoneValue: string, dialCode: string | null | undefined) {
            const normalizedDialCode = normalizeDialCode(dialCode);
            return phoneValue.startsWith(normalizedDialCode)
                ? phoneValue.substring(normalizedDialCode.length)
                : phoneValue;
        }

        function applyMask(input: HTMLInputElement) {
            //@ts-ignore
            if (input.inputmask) {
                //@ts-ignore
                input.inputmask.remove();
            }

            //@ts-ignore
            Inputmask({
                mask: `999999[9]9999`,
                greedy: false,
                placeholder: " ",
                autoUnmask: false
            }).mask(input);
        }

        function syncHiddenInput(input: HTMLInputElement, select: HTMLSelectElement) {
            const hiddenInput = $(input).closest('.input-group').find('.jj-phone-hidden-input')[0] as HTMLInputElement;
            if (!hiddenInput) return;

            const selectedOption = select.selectedOptions[0];
            const dialCode = normalizeDialCode(selectedOption?.getAttribute('dial-code'));
            const localPhone = (input.value || '').replace(/\s/g, '');
            hiddenInput.value = localPhone ? `${dialCode}${localPhone}` : '';
        }

        const selects = document.querySelectorAll<HTMLSelectElement>(`${selectorPrefix}select.jj-phone-select`);
        selects.forEach(select => 
        {
            $(select).on('changed.bs.select', function (e, clickedIndex, isSelected, previousValue) 
            {
                if(isSelected){
                    const input = $(select).closest('.input-group').find('.jj-phone-input')[0] as HTMLInputElement;
                    if (!input) return;

                    applyMask(input);
                    syncHiddenInput(input, select);
                }
            });
        })

        const inputs = document.querySelectorAll<HTMLInputElement>(`${selectorPrefix}input.jj-phone-input`);
        inputs.forEach(input =>
        {
            const select = $(input).closest('.input-group').find('select.jj-phone-select')[0] as HTMLSelectElement;
            if (!select) return;

            const hiddenInput = $(input).closest('.input-group').find('.jj-phone-hidden-input')[0] as HTMLInputElement;
            
            const options = [...select.options];
            const longestDialCode = options.reduce((longest, current) => {
                const dialCode = normalizeDialCode(current.getAttribute('dial-code'));
                return dialCode.length > longest ? dialCode.length : longest;
            }, 0);

            function getCountryFromInputValue(currentValue:string){
                let countryFound: HTMLOptionElement | undefined;
                let verifySubStrLen = longestDialCode;
                while (!countryFound && verifySubStrLen > 0) {
                    const val = currentValue.substring(0, verifySubStrLen);
                    countryFound = options.find(option => {
                        const dialCode = normalizeDialCode(option.getAttribute('dial-code'));
                        return val == dialCode;
                    });
                    verifySubStrLen--;
                }
                return countryFound;
            }

            if(hiddenInput?.value.startsWith('+'))
            {
                const optionCountrySelected = getCountryFromInputValue(hiddenInput.value);
                if(optionCountrySelected)
                    $(select).selectpicker('val', optionCountrySelected.value);
            }

            const selectedOption = select.selectedOptions[0] || options.find(option => option.value === $(select).val());
            if (selectedOption) {
                input.value = getLocalPhoneValue(hiddenInput?.value || input.value || '', selectedOption.getAttribute('dial-code'));
            }

            applyMask(input);
            syncHiddenInput(input, select);

            $(input).on('input', function ()
            {
                syncHiddenInput(this as HTMLInputElement, select);
            });
        })
    }
}
