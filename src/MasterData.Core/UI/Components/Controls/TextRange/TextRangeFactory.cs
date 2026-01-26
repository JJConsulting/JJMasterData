using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components.TextRange;

internal sealed class TextRangeFactory(
        IHttpContext httpContext,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        TextGroupFactory textGroupFactory
        )
    : IControlFactory<JJTextRange>
{

    private IHttpContext HttpContext { get; } = httpContext;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private TextGroupFactory TextGroupFactory { get; } = textGroupFactory;

    public JJTextRange Create()
    {
        return new JJTextRange(HttpContext.Request.Form,StringLocalizer);
    }

    public JJTextRange Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var values = context.FormStateData.Values;
        
        values.TryGetValue($"{field.Name}_to", out var fromValue);

        var range = Create();
        range.FieldType = field.DataType;
        range.FromField = TextGroupFactory.Create(field);
        range.FromField.Text = fromValue?.ToString();
        range.FromField.Name = $"{field.Name}_from";
        range.FromField.PlaceHolder = StringLocalizer["from"].Value.FirstCharToUpper(); //workaround devido a não ser possível ter 2 From no arquivo de resource.

        values.TryGetValue($"{field.Name}_to", out var toValue);

        range.ToField = TextGroupFactory.Create(field);
        range.ToField.Text = toValue?.ToString();
        range.ToField.Name = $"{field.Name}_to";
        range.ToField.PlaceHolder = StringLocalizer["To"];

        return range;
    }
}