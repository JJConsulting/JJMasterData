class CheckboxHelper{
    static check(name:string, value){
        const checkbox = document.querySelector<HTMLInputElement>(`#${name}`);
        
        if(checkbox?.checked){
            document.querySelector<HTMLInputElement>(`#${name}-hidden`).value = value;
        }
    }
}