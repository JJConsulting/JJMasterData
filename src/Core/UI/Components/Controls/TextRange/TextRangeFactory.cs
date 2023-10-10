using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components.TextRange;

internal class TextRangeFactory : IControlFactory<JJTextRange>
{

    private IHttpContext HttpContext { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private TextGroupFactory TextGroupFactory { get; }

    public TextRangeFactory(IHttpContext httpContext,
                            IStringLocalizer<JJMasterDataResources> stringLocalizer,
                            TextGroupFactory textGroupFactory)
    {
        HttpContext = httpContext;
        StringLocalizer = stringLocalizer;
        TextGroupFactory = textGroupFactory;
    }

    public JJTextRange Create()
    {
        return new JJTextRange(HttpContext.Request.Form, TextGroupFactory,StringLocalizer);
    }

    public JJTextRange Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var values = context.FormStateData.Values;
        string valueFrom = "";
        if (values.ContainsKey($"{field.Name}_from"))
        {
            valueFrom = values[$"{field.Name}_from"].ToString();
        }

        var range = Create();
        range.FieldType = field.DataType;
        range.FromField = TextGroupFactory.CreateTextDate();
        range.FromField.Text = valueFrom;
        range.FromField.Name = $"{field.Name}_from";
        range.FromField.PlaceHolder = StringLocalizer["From"];

        string valueTo = "";
        if (values.ContainsKey($"{field.Name}_to"))
        {
            valueTo = values[$"{field.Name}_to"].ToString();
        }

        range.ToField = TextGroupFactory.CreateTextDate();
        range.ToField.Text = valueTo;
        range.ToField.Name = $"{field.Name}_to";
        range.ToField.PlaceHolder = StringLocalizer["To"];

        return range;
    }
}