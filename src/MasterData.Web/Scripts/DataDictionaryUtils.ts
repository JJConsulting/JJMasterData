class DataDictionaryUtils {
    static getAntiforgeryHeaders(url: string, headers?: HeadersInit): Headers {
        const requestHeaders = new Headers(headers);
        const token = document.querySelector<HTMLMetaElement>('meta[name="jjmasterdata-antiforgery"]');
        if (token?.content && token.dataset.headerName && new URL(url, location.href).origin === location.origin) {
            requestHeaders.set(token.dataset.headerName, token.content);
        }
        return requestHeaders;
    }

    static post(url: string, options: RequestInit = {}): Promise<Response> {
        return fetch(url, { ...options, method: "POST", headers: this.getAntiforgeryHeaders(url, options.headers) });
    }

    static sortAction(context: string, url: string, errorMessage: string): void {
        $("#sortable-" + context).sortable({
            update: function () {
                const order = $(this)
                    .sortable('toArray')
                    .map((id) => id.startsWith(context + "_") ? id.substring((context + "_").length) : id);
                const formData = new FormData();
                formData.append('fieldsOrder', order);
                formData.append('context', context);
                DataDictionaryUtils.post(url, {
                    body: formData,
                })
                    .then(function (response) {
                        return response.json();
                    })
                    .then(function (data) {
                        if (!data.success) {
                            messageBox.show('JJMasterData', errorMessage, 4);
                        }
                    });
            }
        });
    }

    static toggleActionEnabled(visibility: boolean, url: string, errorMessage: string): void {
        const formData = new FormData();
        formData.append('visibility', visibility.toString());
        this.post(url, {
            body: formData,
        })
            .then(response => response.json())
            .then(data => {
                if (!data.success) {
                    messageBox.show("JJMasterData", errorMessage, 4);
                }
            })
    }

    static exportElement(id, url, validationMessage) {
        const values = document.querySelector<HTMLInputElement>('#grid-view-selected-rows-' + id).value;
        
        if (values === "") {
            messageBox.show("JJMasterData", validationMessage, 3);
            return false;
        }
        
        SpinnerOverlay.show();
        const requestOptions = getRequestOptions();

        fetch(url, requestOptions).then(async response=>{
            const blob = await response.blob()
            const contentDisposition = response.headers.get('Content-Disposition');
            const fileNameMatch = /filename="(.*)"/.exec(contentDisposition);
            const fileName = fileNameMatch[1];
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            a.click();
            window.URL.revokeObjectURL(url);
            SpinnerOverlay.hide()
        });
    }
    
    static async showAbout(url) {
        await OffcanvasHelper.showOffcanvas("about-offcanvas",url);
    }
}
