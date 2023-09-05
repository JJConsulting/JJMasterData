class LookupHelper {
    static setLookupValues(fieldName: string, id: string, description: string) {
        defaultModal.hide();

        const idInput = document.querySelector<HTMLInputElement>("#" + fieldName );
        idInput.value = id;
        
        const descriptionInput   = document.querySelector<HTMLInputElement>("#" + fieldName + "-description");
        
        if(descriptionInput){
            descriptionInput.value = description;
        }
    }
}