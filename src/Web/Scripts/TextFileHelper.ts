class TextFileHelper {
    static showUploadView(fieldName: string, title: string, routeContext: string) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext)
        urlBuilder.addQueryParameter("fieldName", fieldName)
        const url = urlBuilder.build();

        const modalId = fieldName + "-upload-modal";

        const modal = new Modal();
        modal.modalId = modalId;
        const requestOptions = getRequestOptions();

        modal.showUrl({
            url: url,
            requestOptions: requestOptions
        }, title, ModalSize.ExtraLarge).then(_ => {
            listenAllEvents("#" + modalId)
        })
    }

    static refreshInputs(id: string, presentationText: string, valueText: string) {
        const presentationElement = window.parent.document.getElementById(`${id}-presentation`) as HTMLInputElement;
        const valueElement = window.parent.document.getElementById(id) as HTMLInputElement;

        if (presentationElement) {
            presentationElement.value = presentationText;
        }

        if (valueElement) {
            valueElement.value = valueText;
        }
    }
}