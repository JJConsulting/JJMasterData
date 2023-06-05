class UrlBuilder {
    private queryParameters: Map<string, string>;

    constructor() {
        this.queryParameters = new Map();
    }

    addQueryParameter(key: string, value: string) {
        this.queryParameters.set(key, value);
    }

    build() {
        const form = document.querySelector("form");
        let url = form.getAttribute("action");

        if (!url.includes("?")) {
            url += "?";
        }

        let isFirst = true;
        
        for (const [key, value] of this.queryParameters.entries()) {
            if (!isFirst) {
                url += "&";
            }
            url += `${encodeURIComponent(key)}=${encodeURIComponent(value)}`;
            isFirst = false;
        }

        return url;
    }

}
