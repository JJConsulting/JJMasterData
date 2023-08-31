class LookupHelper{
    static setLookupValues(fieldName, value){
        window.parent.defaultModal.hide();
        setTimeout(function () {
            window.parent.$("#id_" + fieldName).val(value);
            window.parent.$("#" + fieldName).val(value).change().blur();
        }, 100);
    }
}