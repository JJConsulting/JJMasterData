declare const TomSelect: any;

type TomSelectElement = HTMLInputElement | HTMLSelectElement;

class TomSelectHelper {
    static listen(selectorPrefix = String()) {
        document.querySelectorAll<TomSelectElement>(`${selectorPrefix} select.tom-select, ${selectorPrefix} input.tom-select`)
            .forEach(element => this.initialize(element));
    }

    static initialize(element: TomSelectElement) {
        if ((element as any).tomselect) return (element as any).tomselect;

        const isTagsInput = element instanceof HTMLInputElement && element.dataset.tomSelectTags === 'true';
        const isMultiSelect = element instanceof HTMLSelectElement && element.multiple;
        if (isTagsInput)
            element.value = element.value
                .split(',')
                .map(value => value.trim())
                .filter(Boolean)
                .join(',');

        const placeholder = element.dataset.placeholder
            ?? element.getAttribute('placeholder')
            ?? (element instanceof HTMLSelectElement
                ? element.querySelector<HTMLOptionElement>('option[value=""]')?.textContent?.trim()
                : undefined)
            ?? String();

        const tomSelect = new TomSelect(element, {
            allowEmptyOption: !isTagsInput,
            hidePlaceholder: false,
            hideSelected: false,
            maxOptions: null,
            placeholder,
            ...(isTagsInput
                ? {
                    create: true,
                    delimiter: ','
                }
                : {
                    create: false,
                    controlInput: null,
                    searchField: [],
                    plugins: isMultiSelect ? [ 'checkbox_options'] : []
                }),
            render: {
                no_results: () => `<div class="no-results">${element.dataset.noResultsText ?? 'No results found.'}</div>`,
                option: (data, escape) => this.renderOption(element, data, escape, true),
                item: (data, escape) => this.renderOption(element, data, escape, false)
            }
        });

        if (!isTagsInput)
            this.enableTypeAhead(tomSelect);

        return tomSelect;
    }

    static destroy(element: TomSelectElement) {
        (element as any).tomselect?.destroy();
    }

    static setValue(element: TomSelectElement, value: string | string[]) {
        const tomSelect = this.initialize(element);
        tomSelect.setValue(value);
    }

    static clear(element: TomSelectElement) {
        const tomSelect = this.initialize(element);
        tomSelect.clear();
    }

    private static enableTypeAhead(tomSelect: any) {
        const typeAheadTimeout = 750;
        let typedText = String();
        let resetTimeout: number | undefined;

        const resetTypedText = () => {
            typedText = String();
            if (resetTimeout !== undefined) {
                window.clearTimeout(resetTimeout);
                resetTimeout = undefined;
            }
        };

        tomSelect.control.addEventListener('keydown', (event: KeyboardEvent) => {
            if (event.defaultPrevented
                || event.ctrlKey
                || event.metaKey
                || event.altKey
                || event.key.length !== 1
                || tomSelect.isLocked
                || tomSelect.isDisabled
                || tomSelect.isReadOnly)
                return;

            event.preventDefault();

            const typedCharacter = this.normalizeSearchText(event.key);
            if (!typedCharacter)
                return;

            if (resetTimeout !== undefined)
                window.clearTimeout(resetTimeout);

            typedText += typedCharacter;
            let matchingOption = this.findTypeAheadOption(tomSelect, typedText);

            if (!matchingOption && typedText.length > typedCharacter.length) {
                typedText = typedCharacter;
                matchingOption = this.findTypeAheadOption(tomSelect, typedText);
            }

            resetTimeout = window.setTimeout(resetTypedText, typeAheadTimeout);

            if (!matchingOption)
                return;

            if (!tomSelect.isOpen)
                tomSelect.open();

            tomSelect.setActiveOption(matchingOption);
        });

        tomSelect.on('blur', resetTypedText);
        tomSelect.on('destroy', resetTypedText);
    }

    private static findTypeAheadOption(tomSelect: any, typedText: string): HTMLElement | undefined {
        const labelField = tomSelect.settings.labelField;

        return Array.from(tomSelect.selectable() as NodeListOf<HTMLElement>)
            .find(option => {
                const value = option.dataset.value;
                const label = value === undefined ? undefined : tomSelect.options[value]?.[labelField];

                return this.normalizeSearchText(String(label ?? option.textContent ?? String()))
                    .startsWith(typedText);
            });
    }

    private static normalizeSearchText(value: string) {
        return value
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, String())
            .toLocaleLowerCase();
    }

    private static renderOption(
        element: TomSelectElement,
        data: any,
        escape: (value: string) => string,
        isDropdownItem: boolean) {
        const option = element instanceof HTMLSelectElement
            ? [...element.options].find(item => item.value === data.value)
            : undefined;
        const content = option?.dataset.content;

        const cssClass = isDropdownItem ? 'ts-option-content' : String();
        const itemContent = content ?? escape(data.text);

        return `<div class="${cssClass}">${itemContent}</div>`;
    }
}
