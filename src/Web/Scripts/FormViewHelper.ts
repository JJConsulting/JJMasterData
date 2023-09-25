class FormViewHelper {
    static showInsertSuccess(componentName: string, gridViewRouteContext: string) {
        const insertAlertDiv = document.getElementById("insert-alert-div-item");
        
        setTimeout(function () {
            insertAlertDiv.style.opacity = "0";
        }, 1000); 
        
        setTimeout(function () {
            insertAlertDiv.style.display = "none";
        }, 3000);

        GridViewHelper.refresh(componentName,gridViewRouteContext)
    }
    
    static setPanelState(componentName: string,pageState: PageState, routeContext: string){
        document.querySelector<HTMLInputElement>(`#form-view-panel-state-${componentName}`).value = pageState.toString();
        document.querySelector<HTMLInputElement>(`#form-view-action-map-${componentName}`).value = String();
        
        const url = new UrlBuilder().addQueryParameter("routeContext",routeContext).build();
        
        postFormValues({url:url, success:(data)=>{
            HTMLHelper.setInnerHTML(componentName,data);
        }});
    }

    static insertSelection(componentName: string, insertValues: string, routeContext: string) {
        const selectActionValuesInput = document.querySelector(`#form-view-insert-selection-values-${componentName}`) as HTMLInputElement;
        selectActionValuesInput.value = insertValues;
        
        const url = new UrlBuilder().addQueryParameter("routeContext",routeContext).build();
        
        postFormValues({url:url, success:(data)=>{
            HTMLHelper.setInnerHTML(componentName,data);
        }})
    }
}
