class MessageToastHelper{
    static showWhenDOMLoaded(name: string){
        document.addEventListener("DOMContentLoaded", ()=>this.show(name))
    }
    static show(name: string){
        const toastElement = document.getElementById(name);
        const toast = bootstrap.Toast.getOrCreateInstance(toastElement);
        toast.show();
    }
}