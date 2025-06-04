enum PageState {
    List = 1,
    View = 2,
    Insert = 3,
    Update = 4,
    Filter = 5,
    Import = 6,
    Delete = 7
}

const setPageState = (componentName: string, pageState: PageState) =>{
    onDOMReady(function(){
        document.querySelector<HTMLInputElement>(`#form-view-page-state-${componentName}`).value = pageState.toString()
    });
}