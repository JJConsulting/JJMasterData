using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

public class CodeEditorFactory(IFormValues formValues) : IControlFactory<JJCodeEditor>
{
    public JJCodeEditor Create()
    {
        return new JJCodeEditor(formValues);
    }

    public JJCodeEditor Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var codeEditor = Create();
        codeEditor.Name = field.Name;
        codeEditor.Visible = true;
        codeEditor.Text = context.Value?.ToString();
        
        return codeEditor;
    }
}