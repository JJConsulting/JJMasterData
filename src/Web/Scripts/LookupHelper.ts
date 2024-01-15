class LookupHelper {
    static setLookupValues(fieldName: string, id: string, description: string) {
        const idInput = window.parent.document.querySelector<HTMLInputElement>("#" + fieldName );
        idInput.value = id;
        
        const descriptionInput= window.parent.document.querySelector<HTMLInputElement>("#" + fieldName + "-description");
        
        if(descriptionInput){
            descriptionInput.value = description;
        }

        FeedbackIcon.setIcon("#" + fieldName, FeedbackIcon.successClass);

        window.parent.defaultModal.hide();
    }
}