using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

internal class CheckBoxFactory : IControlFactory<JJCheckBox>
{
    private IHttpContext HttpContext { get; }
    private IExpressionsService ExpressionsService { get; }

    public CheckBoxFactory(IHttpContext httpContext, IExpressionsService expressionsService)
    {
        HttpContext = httpContext;
        ExpressionsService = expressionsService;
    }

    public JJCheckBox Create() => new(HttpContext);
    public JJCheckBox Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var check = new JJCheckBox(HttpContext)
        {
            Name = field.Name,
            IsChecked = ExpressionsService.ParseBool(context.FormStateData),
            ToolTip = field.HelpDescription
        };
        return check;
    }
}