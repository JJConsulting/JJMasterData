class CheckboxHelper{
    static check(name:string){
        const checkbox = document.querySelector<HTMLInputElement>(`#${name}-checkbox`);
        
        if(checkbox?.checked){
            document.querySelector<HTMLInputElement>(`#${name}`).value = "true";
        }
        else{
            document.querySelector<HTMLInputElement>(`#${name}`).value = "false";
        }
    }
}