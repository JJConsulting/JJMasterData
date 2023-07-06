using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

public class CheckBoxFactory
{
    private IHttpContext HttpContext { get; }
    private IExpressionsService ExpressionsService { get; }

    public CheckBoxFactory(IHttpContext httpContext, IExpressionsService expressionsService)
    {
        HttpContext = httpContext;
        ExpressionsService = expressionsService;
    }
    
    internal JJCheckBox CreateCheckBox(FormElementField field, object value)
    {
        var check = new JJCheckBox(HttpContext)
        {
            Name = field.Name,
            IsChecked = ExpressionsService.ParseBool(value),
            ToolTip = field.HelpDescription
        };
        return check;
    }
}