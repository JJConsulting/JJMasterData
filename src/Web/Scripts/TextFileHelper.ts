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
        SpinnerOverlay.show();

        modal.showUrl({
            url: url,
            requestOptions: requestOptions
        }, title, ModalSize.ExtraLarge).then(_ => {
            SpinnerOverlay.hide();
            listenAllEvents("#" + modalId)
        })
    }

    static refresh(fieldName: string, routeContext: string) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext)
        urlBuilder.addQueryParameter("fieldName", fieldName)
        const url = urlBuilder.build();

        postFormValues({url:url,success:function(html){
            const modalId = fieldName + "-upload-modal";
            const modal = document.getElementById(modalId);
            const modalBody: HTMLElement = modal.querySelector('.modal-body');
            HTMLHelper.setInnerHTML(modalBody, html);
            listenAllEvents("#" + modalId);
        }});
    }

    static refreshInputs(id: string, presentationText: string, valueText: string) {
        const presentationElement = document.getElementById(`${id}-presentation`) as HTMLInputElement;
        const valueElement =document.getElementById(id) as HTMLInputElement;

        if (presentationElement) {
            presentationElement.value = presentationText;
        }

        if (valueElement) {
            valueElement.value = valueText;
        }
    }
}