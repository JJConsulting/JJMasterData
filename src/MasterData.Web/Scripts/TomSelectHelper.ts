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

        return new TomSelect(element, {
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
                    plugins: isMultiSelect ? [ 'checkbox_options'] : []
                }),
            render: {
                no_results: () => `<div class="no-results">${element.dataset.noResultsText ?? 'No results found.'}</div>`,
                option: (data, escape) => this.renderOption(element, data, escape, true),
                item: (data, escape) => this.renderOption(element, data, escape, false)
            }
        });
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
