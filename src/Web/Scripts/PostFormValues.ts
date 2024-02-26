class PostFormValuesOptions{
    url : string;
    success: (data: string | any ) => void;
    error?: (errorMessage: string) => void;
}

function getRequestOptions() {
    const formData = new FormData(document.querySelector("form"));

    return {
        method: "POST",
        body: formData
    };
}

function postFormValues(options : PostFormValuesOptions) {
    SpinnerOverlay.show();
    const requestOptions = getRequestOptions();
    fetch(options.url, requestOptions)
        .then(response => {
            if (response.headers.get("content-type")?.includes("application/json")) {
                return response.json();
            }
            else if (response.redirected) {
                window.location.href = response.url;
            }
            else if(response.status == 440 || response.status == 403 || response.status == 401)
            {
                //Let the application handle any error in these status
                document.forms[0].submit();
            }
            else {
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