function listenExpressionType(name, hintList) {
    document.getElementById(name + '-ExpressionType').addEventListener('change', function () {
        const selectedType = (this as HTMLInputElement).value;
        const expressionValueInput = document.getElementById(name + '-ExpressionValue') as HTMLInputElement;
        if (selectedType === 'sql') {
            const textArea = document.createElement('textarea');
            textArea.setAttribute('name', name + '-ExpressionValue');
            textArea.setAttribute('id', name + '-ExpressionValue');
            textArea.setAttribute('class', 'form-control');
            textArea.innerText = expressionValueInput.value;
            expressionValueInput.outerHTML = textArea.outerHTML;
            CodeMirrorWrapper.setupCodeMirror(name + '-ExpressionValue', { mode: 'text/x-sql', singleLine: true, hintList: hintList, hintKey: '{' });
        } else {
            const input = document.createElement('input');
            input.setAttribute('type', 'text');
            input.setAttribute('class', 'form-control');
            input.setAttribute('name', name + '-ExpressionValue');
            input.setAttribute('id', name + '-ExpressionValue');
            input.value = expressionValueInput.value;

            // @ts-ignore
            if (expressionValueInput.codeMirrorInstance) {
                // @ts-ignore
                expressionValueInput.codeMirrorInstance.setOption('mode', 'text/x-csrc');
                // @ts-ignore
                expressionValueInput.codeMirrorInstance.getWrapperElement().parentNode.removeChild(expressionValueInput.codeMirrorInstance.getWrapperElement());
            }

            expressionValueInput.outerHTML = input.outerHTML;
        }
    });
}