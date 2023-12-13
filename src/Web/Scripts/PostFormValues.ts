class PostFormValuesOptions{
    url : string;
    success: (data: string | any ) => void;
    error?: (errorMessage: string) => void;
}

function postFormValues(options : PostFormValuesOptions) {
    SpinnerOverlay.show();
    const formData = $("form").serialize();
    
    const requestOptions = {
        method: "POST",
        body: formData,
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
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