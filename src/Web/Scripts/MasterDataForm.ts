function getMasterDataForm(){
    const mdForm: HTMLFormElement = document.getElementById("masterdata-form") as HTMLFormElement; 
    
    if(mdForm)
        return mdForm;
    
    return document.forms[0];
}