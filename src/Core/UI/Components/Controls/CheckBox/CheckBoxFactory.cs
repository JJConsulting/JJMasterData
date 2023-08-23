using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

internal class CheckBoxFactory : IControlFactory<JJCheckBox>
{
    private IHttpContext HttpContext { get; }

    public CheckBoxFactory(IHttpContext httpContext)
    {
        HttpContext = httpContext;
    }

    public JJCheckBox Create() => new(HttpContext);
    public JJCheckBox Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var check = new JJCheckBox(HttpContext)
        {
            Name = field.Name,
            IsChecked = StringManager.ParseBool(context.Value),
            ToolTip = field.HelpDescription
        };
        return check;
    }
}