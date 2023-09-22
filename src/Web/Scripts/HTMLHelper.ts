class HTMLHelper {
    static setInnerHTML(element: string | HTMLElement, html: string): void {
        const targetElement = typeof element === "string" ? document.getElementById(element) : element;
        if (!targetElement) {
            throw new Error(`Element not found: ${element}`);
        }

        targetElement.innerHTML = html;

        Array.from(targetElement.querySelectorAll("script")).forEach((oldScriptElement: HTMLScriptElement) => {
            const newScriptElement = document.createElement("script");

            Array.from(oldScriptElement.attributes).forEach((attr) => {
                newScriptElement.setAttribute(attr.name, attr.value);
            });

            const scriptText = document.createTextNode(oldScriptElement.innerHTML);
            newScriptElement.appendChild(scriptText);

            oldScriptElement.parentNode?.replaceChild(newScriptElement, oldScriptElement);
        });
    }
}
