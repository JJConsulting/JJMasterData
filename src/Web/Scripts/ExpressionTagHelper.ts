function listenExpressionType(name, hintList, isBoolean) {
    document.getElementById(name + '-ExpressionType').addEventListener('change', function () {
        const selectedType = (this as HTMLInputElement).value;
        const expressionValueInput = document.getElementById(name + '-ExpressionValue') as HTMLInputElement;
        const expressionValueEditor = document.getElementById(name + '-ExpressionValueEditor') as HTMLInputElement;
        
        if(selectedType ==='val' && isBoolean === true){
            const div = document.createElement('div');
            div.classList.add('form-switch', 'form-switch-md', 'form-check');

            
            const expressionValueInputName = name + '-ExpressionValue';
            
            const input = document.createElement('input') as HTMLInputElement;
            input.name = expressionValueInputName;
            input.id = expressionValueInputName;
            input.setAttribute("value","false")
            input.setAttribute("hidden","hidden")

            const checkbox = document.createElement('input') as HTMLInputElement;
            checkbox.name = name + '-ExpressionValue-checkbox';
            checkbox.id = name + '-ExpressionValue-checkbox';
            checkbox.type = 'checkbox';
            checkbox.setAttribute("role", 'switch');
            checkbox.setAttribute("value","false");
            checkbox.setAttribute("onchange",`CheckboxHelper.check('${expressionValueInputName}')`)
            checkbox.classList.add('form-check-input');
            
            div.appendChild(input);
            div.appendChild(checkbox);
            
            
            expressionValueEditor.innerHTML = div.outerHTML;
        }
        else {
            const textArea = document.createElement('textarea');
            textArea.setAttribute('name', name + '-ExpressionValue');
            textArea.setAttribute('id', name + '-ExpressionValue');
            textArea.setAttribute('class', 'form-control');
            textArea.innerText = expressionValueInput.value;
            expressionValueEditor.innerHTML = textArea.outerHTML;
            CodeMirrorWrapper.setupCodeMirror(name, { mode: 'text/x-sql', singleLine: true, hintList: hintList, hintKey: '{' });
        }
    });
}