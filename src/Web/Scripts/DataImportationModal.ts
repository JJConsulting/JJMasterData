class DataImportationModal {
    
    static instance : Modal
    static getInstance() {
        if(this.instance === undefined){
            this.instance = new Modal();
            this.instance.onModalHidden = DataImportationHelper.removePasteListener;
        }
        return this.instance;
    }
}
