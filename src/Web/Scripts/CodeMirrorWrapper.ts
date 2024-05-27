// @ts-nocheck

class CodeMirrorWrapperOptions{
    mode: string
    hintList: string
    hintKey: string
    singleLine: boolean
    size: number
}

class CodeMirrorWrapper{
    private static isCodeMirrorConfigured(textArea) {
        // ReSharper disable once TsNotResolved
        return textArea.codeMirrorInstance != null;
    }
    static setupCodeMirror(elementId: string, options: CodeMirrorWrapperOptions) {
        const textArea = document.getElementById(elementId);
        
        if(!textArea)
            return;
        
        if (this.isCodeMirrorConfigured(textArea))
            return;
        
        const codeMirrorTextArea = CodeMirror.fromTextArea(textArea, {
            mode: options.mode,
            indentWithTabs: true,
            smartIndent: true,
            lineNumbers: !options.singleLine,
            autofocus: false,
            autohint: true,
            scrollbarStyle: options.singleLine ?  "null" : "native", //yes, the null string.
            viewportMargin: options.singleLine ? Infinity : null,
            extraKeys: { "Ctrl-Space": "autocomplete" }
        });

        if (options.singleLine) {
            const inputElement = document.querySelector(".form-select");
            const inputStyle = window.getComputedStyle(inputElement);
            codeMirrorTextArea.getWrapperElement().style.height = inputStyle.height;
           
            codeMirrorTextArea.on("beforeChange", function(instance, change) {
               const newText = change.text.join("").replace(/\n/g, "");
               change.update(change.from, change.to, [newText]);
               return true;
            });
        }
        else{
            codeMirrorTextArea.setSize(null, options.size ?? 250);
        }

        
        // @ts-ignore
        CodeMirror.registerHelper('hint', 'hintList', function (_) {
            const cur = codeMirrorTextArea.getCursor();
            return {
                list: options.hintList,
                from: CodeMirror.Pos(cur.line, cur.ch),
                to: CodeMirror.Pos(cur.line, cur.ch)
            }
        });

        codeMirrorTextArea.on("keyup", function (cm, event) {
            if (!cm.state.completionActive && event.key === options.hintKey) {
                CodeMirror.commands.autocomplete(cm, CodeMirror.hint.hintList, { completeSingle: false });
            }
        });
        
        // ReSharper disable once TsNotResolved
        textArea.codeMirrorInstance = codeMirrorTextArea;
        setTimeout(() => {
            codeMirrorTextArea.refresh();
        }, 250);
    }

}