class CheckboxHelper{
    static check(name:string, value){
        const checkbox = document.querySelector<HTMLInputElement>(`#${name}-checkbox`);
        
        if(checkbox?.checked){
            document.querySelector<HTMLInputElement>(`#${name}`).value = value;
        }
        else{
            document.querySelector<HTMLInputElement>(`#${name}`).value = "false";
        }
    }
}