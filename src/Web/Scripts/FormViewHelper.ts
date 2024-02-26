class FormViewHelper {
    static showInsertSuccess(componentName: string, gridViewRouteContext: string) {
        const insertAlertDiv = document.getElementById(`insert-alert-div-${componentName}`);
        
        setTimeout(function () {
            insertAlertDiv.style.opacity = "0";
        }, 1000); 
        
        setTimeout(function () {
            insertAlertDiv.style.display = "none";
        }, 3000);

        GridViewHelper.refresh(componentName,gridViewRouteContext)
    }

    private static refreshFormView(componentName: string, routeContext: string) {
        const url = new UrlBuilder().addQueryParameter("routeContext", routeContext).build();

        postFormValues({
            url: url,
            success: (data) => {
                TooltipHelper.dispose("#" + componentName)
                HTMLHelper.setOuterHTML(componentName, data);
                listenAllEvents("#" + componentName);
            }
        });
    }

    static setPageState(componentName: string, pageState: PageState, routeContext: string) {
        document.querySelector<HTMLInputElement>(`#form-view-page-state-${componentName}`).value = pageState.toString();
        document.querySelector<HTMLInputElement>(`#current-action-map-${componentName}`).value = String();

        this.refreshFormView(componentName, routeContext);
    }

    static setPanelState(componentName: string, pageState: PageState, routeContext: string) {
        document.querySelector<HTMLInputElement>(`#form-view-panel-state-${componentName}`).value = pageState.toString();
        document.querySelector<HTMLInputElement>(`#current-action-map-${componentName}`).value = String();

        this.refreshFormView(componentName, routeContext);
    }

    static insertSelection(componentName: string, insertValues: string, routeContext: string) {
        document.querySelector<HTMLInputElement>(`#form-view-insert-selection-values-${componentName}`).value = insertValues;

        this.refreshFormView(componentName, routeContext);
    }
}
