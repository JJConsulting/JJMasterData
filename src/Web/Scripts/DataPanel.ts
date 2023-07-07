class DataPanel {
    static Reload(url, componentName, fieldName) {
        const form = document.querySelector("form");
        fetch(url, {
            method: form.method,
            body: new FormData(form),
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(response.statusText);
                }
                return response.text();
            })
            .then(data => {
                document.getElementById(componentName).innerHTML = data;
                jjloadform();
                jjutil.gotoNextFocus(fieldName);
            })
            .catch(error => {
                console.error(error);
            });
    }
}
