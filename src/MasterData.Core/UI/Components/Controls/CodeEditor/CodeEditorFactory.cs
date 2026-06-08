using JJMasterData.Core.DataDictionary.Models;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.UI.Components;

public class CodeEditorFactory(IHttpContextAccessor formValues) : IControlFactory<JJCodeEditor>
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