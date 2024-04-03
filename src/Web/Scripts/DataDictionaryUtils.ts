class DataDictionaryUtils {
    static deleteAction(actionName: string, url: string, confirmationMessage: string): void {
        let confirmed = confirm(confirmationMessage);
        if (confirmed == true) {
            postFormValues({
                url:url,
                success:function(data){
                    if (data.success) {
                        document.getElementById(actionName).remove();
                    }
                }
            });
        }
    }

    static sortAction(context: string, url: string, errorMessage: string): void {
        $("#sortable-" + context).sortable({
            update: function () {
                const order = $(this).sortable('toArray');
                const formData = new FormData();
                formData.append('fieldsOrder', order);
                formData.append('context', context);
                fetch(url, {
                    method: 'POST',
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
        }).disableSelection();
    }

    static toggleActionEnabled(visibility: boolean, url: string, errorMessage: string): void {
        const formData = new FormData();
        formData.append('visibility', visibility.toString());
        fetch(url, {
            method: "POST",
            body: formData,
        })
            .then(response => response.json())
            .then(data => {
                if (!data.success) {
                    messageBox.show("JJMasterData", errorMessage, 4);
                }
            })
    }

    static postAction(url: string): void {
        // @ts-ignore
        window.parent.getMasterDataForm().requestSubmit();
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
}
