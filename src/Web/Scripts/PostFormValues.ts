class PostFormValuesOptions{
    url : string;
    success: (response: any ) => void;
    error?: (errorMessage: string) => void;
}

function postFormValues(options : PostFormValuesOptions) {
    SpinnerOverlay.show();
    const formData = new FormData(document.querySelector("form"));
    
    const requestOptions = {
        method: "POST",
        body: formData
    };
    
    fetch(options.url, requestOptions)
        .then(response => {
            if (response.headers.get("content-type")?.includes("application/json")) {
                return response.json();
            } else {
                return response.text();
            }
        })
        .then(data => {
            options.success(data)
        })
        .catch(error => {
            if(options.error){
                options.error(error)
            }
            else{
                console.error(error);
            }
        })
        .then(() => {
            SpinnerOverlay.hide();
        });
}