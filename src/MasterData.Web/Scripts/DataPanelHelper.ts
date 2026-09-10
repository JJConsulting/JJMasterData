type PostBackFieldState = {
    name: string;
    value: string;
};

class DataPanelHelper {
    public static reload(panelName, elementFieldName, fieldNameWithPrefix, routeContext) {
        const nextFieldState = this.getNextPostBackFieldState(panelName, fieldNameWithPrefix);
        const urlBuilder = new UrlBuilder();
        urlBuilder.addQueryParameter("panelName", panelName);
        urlBuilder.addQueryParameter("fieldName", elementFieldName);
        urlBuilder.addQueryParameter("routeContext", routeContext);
        
        postFormValues({
            url: urlBuilder.build(),
            success: data => {
                if (typeof data === "string") {
                    HTMLHelper.setOuterHTML(panelName, data);
                    listenAllEvents("#" + panelName);
                    this.triggerChangeIfValueChanged(panelName, nextFieldState);
                } else {
                    if (data.jsCallback) {
                        eval(data.jsCallback);
                    }
                }
                jjutil.gotoNextFocus(fieldNameWithPrefix);
            }
        });
    }

    private static getNextPostBackFieldState(
        panelName: string,
        currentFieldName: string): PostBackFieldState | null {
        const panel = document.getElementById(panelName);
        if (!panel)
            return null;

        const postBackFields = [...panel.querySelectorAll<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>(
            '[onchange*="DataPanelHelper.reload"]')];
        const currentFieldIndex = postBackFields.findIndex(field =>
            field.id === currentFieldName || field.name === currentFieldName);

        if (currentFieldIndex === -1)
            return null;

        const currentField = postBackFields[currentFieldIndex];
        const nextField = postBackFields
            .slice(currentFieldIndex + 1)
            .find(field => field.name !== currentField.name && !field.disabled);

        if (!nextField)
            return null;

        return {
            name: nextField.name,
            value: this.getPostedFieldValue(nextField.name)
        };
    }

    private static triggerChangeIfValueChanged(
        panelName: string,
        previousState: PostBackFieldState | null) {
        if (!previousState)
            return;

        const panel = document.getElementById(panelName);
        if (!panel)
            return;

        const field = panel.querySelector<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>(
            `[name="${CSS.escape(previousState.name)}"]`);

        if (!field || field.disabled)
            return;

        const currentValue = this.getPostedFieldValue(previousState.name);
        if (currentValue === previousState.value)
            return;

        field.dispatchEvent(new Event("change", { bubbles: true }));
    }

    private static getPostedFieldValue(fieldName: string) {
        return new FormData(getMasterDataForm())
            .getAll(fieldName)
            .join();
    }
}
