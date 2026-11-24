class PostFormValuesOptions{
    url : string;
    success: (data: string | any ) => void;
    error?: (errorMessage: string) => void;
}

function getRequestHeaders() {
    const token = getMasterDataForm()?.querySelector<HTMLInputElement>('input[name="__RequestVerificationToken"]');
    return {
        "X-Requested-With": "XMLHttpRequest",
        "RequestVerificationToken": token?.value ?? ""
    };
}

function getRequestOptions() {
    const formData = new FormData(getMasterDataForm());

    return {
        method: "POST",
        body: formData,
        headers: getRequestHeaders()
    };
}

function postFormValues(options : PostFormValuesOptions) {
    postValues(options, getRequestOptions());
}

function postContent(options: PostFormValuesOptions, body: BodyInit) {
    postValues(options, {
        method: "POST",
        body: body,
        headers: getRequestHeaders()
    });
}

function postValues(options: PostFormValuesOptions, requestOptions: RequestInit) {
    SpinnerOverlay.show();
    const event = new Event("postFormValuesCompleted");
    fetch(options.url, requestOptions)
        .then(response => {
            if (response.headers.get("content-type")?.includes("application/json")) {
                return response.json();
            }
            else if (response.redirected) {
                window.location.href = response.url;
                return null;
            }
            else if(response.status == 440 || response.status == 403 || response.status == 401)
            {
                //Let the application handle any error in these status
                getMasterDataForm().submit();
            }
            else {
                return response.text();
            }
        })
        .then(data => {
            options.success(data)
            document.dispatchEvent(event);
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

async function postFormValuesAsync(url: string) {
    SpinnerOverlay.show();
    const requestOptions = getRequestOptions();
    try {
        const response = await fetch(url, requestOptions);

        if (response.headers.get("content-type")?.includes("application/json")) {
            return await response.json();
        } else if (response.redirected) {
            window.location.href = response.url;
            return;
        } else if (response.status === 440 || response.status === 403 || response.status === 401) {
            getMasterDataForm().submit();
            return;
        } else {
            return await response.text();
        }
    } finally {
        SpinnerOverlay.hide();
    }
}
