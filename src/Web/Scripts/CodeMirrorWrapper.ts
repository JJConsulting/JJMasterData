// @ts-nocheck

class CodeMirrorWrapperOptions{
    mode: string
    hintList: string
    hintKey: string
}

class CodeMirrorWrapper{
    static setupCodeMirror(elementId: string, options: CodeMirrorWrapperOptions) {
        const codeMirrorTextArea = CodeMirror.fromTextArea(document.querySelector("#"+elementId), {
            mode: options.mode,
            indentWithTabs: true,
            smartIndent: true,
            lineNumbers: true,
            autofocus: true,
            autoRefresh:true,
            autohint: true,
            extraKeys: { "Ctrl-Space": "autocomplete" }
        });

        codeMirrorTextArea.setSize(null, 250);
        
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
            if (!cm.state.completionActive && event.key === "\"") {  
                CodeMirror.commands.autocomplete(cm, CodeMirror.hint.hintList, { completeSingle: false });
            }
        });
    }
}