interface HTMLInputElement {
    dropzone: any;
}

interface Window {
    Dropzone: any;
}

declare const bootstrap: any;

declare function flatpickr(selector: any, options?: any): any;

declare const Dropzone: any;

declare namespace Dropzone {
    type DropzoneOptions = any;
    type DropzoneFile = any;
}

interface JQueryStatic {
    (...args: any[]): any;
    isFunction(value: any): boolean;
}

declare const $: JQueryStatic;
declare const jQuery: JQueryStatic;

interface JQuery{
    bootstrapToggle : Function
    typeahead : Function
    valid: Function
    sortable: Function
}
