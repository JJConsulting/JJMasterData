class HTMLHelper {
    static setOuterHTML(elementName: string, html: string): void {
        const targetElement =  document.getElementById(elementName);
        if (!targetElement) {
            throw new Error(`Element not found: ${elementName}`);
        }

        targetElement.outerHTML = html;

        this.makeScriptsExecutable(document.getElementById(elementName))
    }

    static setInnerHTML(element: string | HTMLElement, html: string): void {
        const targetElement = typeof element === "string" ? document.getElementById(element) : element;
        if (!targetElement) {
            throw new Error(`Element not found: ${element}`);
        }

        targetElement.innerHTML = html;

        this.makeScriptsExecutable(targetElement);
    }
    private static makeScriptsExecutable(element: Element) {
        element.querySelectorAll("script").forEach(script => {
            const clone = document.createElement("script")

            for (const attr of script.attributes) {
                clone.setAttribute(attr.name, attr.value)
            }

            clone.text = script.innerHTML
            script.parentNode?.replaceChild(clone, script)
        })
    }

}
