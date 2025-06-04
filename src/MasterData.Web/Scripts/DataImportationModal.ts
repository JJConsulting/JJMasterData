class DataImportationModal {
    
    static instance : Modal
    static getInstance() {
        if(this.instance === undefined){
            this.instance = new Modal();
            $("body").on('hidden.bs.modal',"#" + this.instance.modalId, function () {
                DataImportationHelper.removePasteListener()
            });
        }
        return this.instance;
    }
}
