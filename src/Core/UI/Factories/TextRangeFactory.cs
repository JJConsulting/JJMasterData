using System.Collections.Generic;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

public class TextRangeFactory
{
    private IHttpContext HttpContext { get; }
    private TextGroupFactory TextGroupFactory { get; }

    public TextRangeFactory(IHttpContext httpContext, TextGroupFactory textGroupFactory)
    {
        HttpContext = httpContext;
        TextGroupFactory = textGroupFactory;
    }
    internal JJTextRange CreateTextRange(FormElementField field, IDictionary<string,dynamic> values)
    {
        string valueFrom = "";
        if (values != null && values.ContainsKey(field.Name + "_from"))
        {
            valueFrom = values[field.Name + "_from"].ToString();
        }
        
        var range = new JJTextRange(HttpContext,TextGroupFactory);
        range.FieldType = field.DataType;
        range.FromField = TextGroupFactory.CreateTextGroup(field);
        range.FromField.Text = valueFrom;
        range.FromField.Name = field.Name + "_from";
        range.FromField.PlaceHolder = Translate.Key("From");
        
        string valueTo = "";
        if (values != null && values.ContainsKey(field.Name + "_to"))
        {
            valueTo = values[field.Name + "_to"].ToString();
        }
        
        range.ToField = TextGroupFactory.CreateTextGroup(field);
        range.ToField.Text = valueTo;
        range.ToField.Name = field.Name + "_to";
        range.ToField.PlaceHolder = Translate.Key("To");

        return range;
    }
}