class CheckboxHelper{
    static check(name:string){
        const checkbox = <HTMLInputElement>document.getElementById(`${name}-checkbox`);

        if(checkbox?.checked){
            (<HTMLInputElement>document.getElementById(name)).value = "true";
        }
        else{
            (<HTMLInputElement>document.getElementById(name)).value = "false";
        }
    }
}