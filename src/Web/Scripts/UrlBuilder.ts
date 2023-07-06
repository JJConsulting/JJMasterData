class UrlBuilder {
    private queryParameters: Map<string, string>;
    private url: string
    
    constructor(url = null) {
        this.url = url;
        this.queryParameters = new Map();
    }
    
    addQueryParameter(key: string, value: string) {
        this.queryParameters.set(key, value);
    }

    build() {
        const form = document.querySelector("form");
        
        if(this.url == null){
            this.url = form.getAttribute("action");
        }
        
        if (!this.url.includes("?")) {
            this.url += "?";
        }

        let isFirst = true;

        for (const [key, value] of this.queryParameters.entries()) {
            if (!isFirst) {
                this.url += "&";
            }
            this.url += `${encodeURIComponent(key)}=${encodeURIComponent(value)}`;
            isFirst = false;
        }

        return this.url;
    }

}