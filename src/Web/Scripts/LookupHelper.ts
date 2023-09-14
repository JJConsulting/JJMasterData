class LookupHelper {
    static setLookupValues(fieldName: string, id: string, description: string) {
        defaultModal.remove();

        const idInput = document.querySelector<HTMLInputElement>("#" + fieldName );
        idInput.value = id;
        
        const descriptionInput   = document.querySelector<HTMLInputElement>("#" + fieldName + "-description");
        
        if(descriptionInput){
            descriptionInput.value = description;
        }

        FeedbackIcon.setIcon(id, FeedbackIcon.successClass);
    }
}