class TextFileHelper {
    static showUploadView(fieldName: string, title: string, routeContext: string, draftInputId: string) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext)
        urlBuilder.addQueryParameter("fieldName", fieldName)
        this.addDraftId(urlBuilder, draftInputId);
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

    static refresh(fieldName: string, routeContext: string, draftInputId: string) {
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("routeContext", routeContext)
        urlBuilder.addQueryParameter("fieldName", fieldName)
        this.addDraftId(urlBuilder, draftInputId);
        const url = urlBuilder.build();

        postFormValues({url:url,success:function(html){
            const uploadViewSelector = "#" + fieldName + "-upload-view";
            const uploadView: HTMLElement = document.querySelector(uploadViewSelector);
            HTMLHelper.setInnerHTML(uploadView, html);
            listenAllEvents(uploadViewSelector);
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

    private static addDraftId(urlBuilder: UrlBuilder, draftInputId: string) {
        const draftInput = document.getElementById(draftInputId) as HTMLInputElement;
        if (draftInput?.value) {
            urlBuilder.addQueryParameter("draftId", draftInput.value);
        }
    }
}
