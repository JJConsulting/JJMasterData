class DataExportation{

    private static setSettingsHTML(componentName, html) {
        const modalBody = document.querySelector("#export_modal_" + componentName + " .modal-body ");
        modalBody.innerHTML = html;
        jjloadform(null);

        const qtdElement = document.querySelector("#" + componentName + "_totrows");
        if (qtdElement) {
            const totRows = +qtdElement.textContent.replace(/\./g, "");
            if (totRows > 50000) {
                document.querySelector<HTMLElement>("#warning_exp_" + componentName).style.display = "block";
            }
        }

        if (bootstrapVersion < 5) {
            $("#export_modal_" + componentName).modal();
        } else {
            const modal = new bootstrap.Modal(document.querySelector("#export_modal_" + componentName), {});
            modal.show();
        }
    }
    static openExportPopup(url: string, componentName: string) {
        fetch(url)
            .then(response => response.text())
            .then(data => {
                this.setSettingsHTML(componentName, data)
            })
            .catch(error => {
                console.log(error);
            });
    }
    static startExportation(url, componentName) {
        fetch(url, {
            method: "POST",
        })
            .then(response => {
                if (response.ok) {
                    return response.text();
                } else {
                    throw new Error("Request failed with status: " + response.status);
                }
            })
            .then(data => {
                const modalBody = document.querySelector("#export_modal_" + componentName + " .modal-body");
                modalBody.innerHTML = data;
                jjloadform(null);
            })
            .catch(error => {
                console.log(error);
            });
    }
}