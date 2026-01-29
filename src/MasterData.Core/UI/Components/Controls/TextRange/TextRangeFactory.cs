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
    public JJTextRange Create()
    {
        return new JJTextRange(httpContext.Request.Form,stringLocalizer);
    }

    public JJTextRange Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var values = context.FormStateData.Values;
        var pageState = context.FormStateData.PageState;
        
        values.TryGetValue($"{field.Name}_from", out var fromValue);

        var range = Create();
        range.FieldType = field.DataType;
        range.FromField = textGroupFactory.Create(field, pageState);
        range.FromField.Text = fromValue?.ToString();
        range.FromField.Name = $"{field.Name}_from";
        range.FromField.PlaceHolder = stringLocalizer["from"].Value.FirstCharToUpper(); //workaround devido a não ser possível ter 2 From no arquivo de resource.

        values.TryGetValue($"{field.Name}_to", out var toValue);

        range.ToField = textGroupFactory.Create(field, pageState);
        range.ToField.Text = toValue?.ToString();
        range.ToField.Name = $"{field.Name}_to";
        range.ToField.PlaceHolder = stringLocalizer["To"];

        return range;
    }
}