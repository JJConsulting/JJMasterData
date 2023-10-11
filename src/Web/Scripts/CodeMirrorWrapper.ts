// @ts-nocheck

class CodeMirrorWrapperOptions{
    mode: string
    hintList: string
    hintKey: string
}

class CodeMirrorWrapper{
    static setupCodeMirror(elementId: string, options: CodeMirrorWrapperOptions) {
        
        const textArea = document.querySelector("#"+elementId);
        
        if(!textArea)
            return;
        
        console.log("hola")
        
        const codeMirrorTextArea = CodeMirror.fromTextArea(textArea, {
            mode: options.mode,
            indentWithTabs: true,
            smartIndent: true,
            lineNumbers: true,
            autofocus: true,
            autoRefresh: true,
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
            if (!cm.state.completionActive && event.key === options.hintKey) {  
                CodeMirror.commands.autocomplete(cm, CodeMirror.hint.hintList, { completeSingle: false });
            }
        });
    }
}