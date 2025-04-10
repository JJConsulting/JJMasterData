class CodeEditor {
    private static monacoLoaded = false;

    static setup(selectorPrefix: string) {
        const editors = document.querySelectorAll(selectorPrefix + '.jj-code-editor');
        if (editors.length === 0)
            return;

        if (!CodeEditor.monacoLoaded) {
            CodeEditor.loadMonaco(editors);
            CodeEditor.monacoLoaded = true;
        } else {
            //@ts-ignore
            require(['vs/editor/editor.main'], function () {
                CodeEditor.initializeEditors(editors);
            });
        }
    }

    private static loadMonaco(editors) {
        const script1 = document.createElement('script');
        script1.src = 'https://cdn.jsdelivr.net/npm/monaco-editor@0.52.2/min/vs/loader.js';
        script1.onload = () => {
            //@ts-ignore
            require.config({ paths: { vs: 'https://cdn.jsdelivr.net/npm/monaco-editor@0.52.2/min/vs' } });
            //@ts-ignore
            require(['vs/editor/editor.main'], function () {
                CodeEditor.observeTheme();
                CodeEditor.loadHints();
                CodeEditor.initializeEditors(editors);
            });
        };
        document.body.appendChild(script1);
    }

    private static observeTheme() {
        const observer = new MutationObserver(mutations => {
            mutations.forEach(mutation => {
                if (mutation.type === "attributes" && mutation.attributeName === "data-bs-theme") {
                    const theme = document.documentElement.getAttribute("data-bs-theme");
                    //@ts-ignore
                    monaco.editor.setTheme(theme === "dark" ? "vs-dark" : "vs");
                }
            });
        });

        observer.observe(document.documentElement, { attributes: true, attributeFilter: ["data-bs-theme"] });
    }

    private static loadHints() {
        const el = document.getElementById("jj-code-editor-hints") as HTMLInputElement;
        if (!el || !el.value) return;

        let hints;
        try {
            hints = JSON.parse(el.value);
        } catch {
            return;
        }

        Object.keys(hints).forEach(lang => {
            //@ts-ignore
            monaco.languages.registerCompletionItemProvider(lang, {
                provideCompletionItems: function(model, position) {
                    const suggestions = hints[lang].map(h => ({
                        label: h.label,
                        kind: 17,
                        insertText: h.insertText,
                        detail: h.details
                    }));
                    return { suggestions };
                }
            });
        });
    }

    private static initializeEditors(editors: NodeListOf<Element>) {
        editors.forEach((el: HTMLElement) => {
            const editorId = el.dataset.editorId;
            const language = el.dataset.language;
            const name = el.dataset.editorName;
            const editorTextArea = document.getElementById(name);

            // @ts-ignore
            const editor = monaco.editor.create(document.getElementById(editorId), {
                // @ts-ignore
                value: editorTextArea.value,
                automaticLayout: true,
                language: language,
                theme: getTheme() === 'dark' ? 'vs-dark' : 'vs'
            });

            editor.getModel().onDidChangeContent(() => {
                // @ts-ignore
                editorTextArea.value = editor.getValue();
            });
        });
    }
}
