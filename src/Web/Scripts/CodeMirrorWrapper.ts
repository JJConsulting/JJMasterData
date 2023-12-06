// @ts-nocheck

class CodeMirrorWrapperOptions{
    mode: string
    hintList: string
    hintKey: string
    singleLine: boolean
}

class CodeMirrorWrapper{
    private static isCodeMirrorConfigured(elementId) {
        const textArea = document.querySelector("#"+elementId);
        
        // ReSharper disable once TsNotResolved
        return textArea.codeMirrorInstance != null;
    }
    static setupCodeMirror(elementId: string, options: CodeMirrorWrapperOptions) {
        
        const textArea = document.querySelector("#"+elementId);
        
        if(!textArea)
            return;
        
        if(this.isCodeMirrorConfigured(elementId))
            return;
        
        const codeMirrorTextArea = CodeMirror.fromTextArea(textArea, {
            mode: options.mode,
            indentWithTabs: true,
            smartIndent: true,
            lineNumbers: !options.singleLine,
            autofocus: false,
            autohint: true,
            extraKeys: { "Ctrl-Space": "autocomplete" }
        });
        
        if(options.singleLine){
            codeMirrorTextArea.setSize(null, 30);
            //codeMirrorTextArea.on("beforeChange", function(instance, change) {
            //    const newText = change.text.join("").replace(/\n/g, "");
            //    change.update(change.from, change.to, [newText]);
            //    return true;
            //});
        }
        else{
            codeMirrorTextArea.setSize(null, 250);
        }
        // ReSharper disable once TsNotResolved
        textArea.codeMirrorInstance = codeMirrorTextArea;
        
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

        setTimeout(() => { codeMirrorTextArea.refresh() }, 200);
    }
}