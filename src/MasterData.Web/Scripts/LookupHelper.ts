class LookupHelper {
    static setLookupValues(fieldName: string, id: string, description: string) {
        const idInput = window.parent.document.querySelector<HTMLInputElement>("#" + fieldName );
        idInput.value = id;
        idInput.dispatchEvent(new Event('change'));
        const descriptionInput= window.parent.document.querySelector<HTMLInputElement>("#" + fieldName + "-description");
        
        if(descriptionInput){
            descriptionInput.value = description;
            descriptionInput.dispatchEvent(new Event('change'));
        }

        FeedbackIcon.setIcon("#" + fieldName, FeedbackIcon.successClass);

        window.parent.defaultModal.hide();
    }
}