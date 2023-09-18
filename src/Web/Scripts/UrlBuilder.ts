class UrlBuilder {
    private queryParameters: Map<string, string>;
    private url: string
    
    constructor(url = null) {
        this.url = url;
        this.queryParameters = new Map();
    }
    
    addQueryParameter(key: string, value: string) {
        this.queryParameters.set(key, value);
        return this;
    }

    build() {
        const form = document.querySelector("form");
        
        if(this.url == null){
            this.url = form.getAttribute("action");
        }
        
        if (!this.url.includes("?")) {
            this.url += "?";
        } else{
            this.url += "&";
        }
        
        const queryParameters = [...this.queryParameters.entries()];
        for (let i = 0; i < queryParameters.length; i++) {
            const [key, value] = queryParameters[i];
            this.url += `${encodeURIComponent(key)}=${encodeURIComponent(value)}`;
            if (i < queryParameters.length - 1) {
                this.url += "&";
            }
        }

        return this.url;
    }

}