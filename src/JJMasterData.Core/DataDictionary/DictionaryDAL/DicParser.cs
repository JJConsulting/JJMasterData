using System.Runtime.Serialization;
using JJMasterData.Commons.Dao.Entity;

namespace JJMasterData.Core.DataDictionary.DictionaryDAL;

[DataContract(Name = "elementInfo")]
public class DicParser
{
    [DataMember(Name = "table")]
    public Element Table { get; set; }

    [DataMember(Name = "form")]
    public DicFormParser Form { get; set; }

    [DataMember(Name = "uioptions")]
    public UIOptions UIOptions { get; set; }

    [DataMember(Name = "api")]
    public ApiSettings Api { get; set; }

    public FormElement GetFormElement()
    {
        if (Table == null)
            return null;

        if (Form == null)
            return null;

        FormElement fe = new FormElement(Table)
        {
            Title = Form.Title,
            SubTitle = Form.SubTitle,
            Panels = Form.Panels
        };

        foreach (var item in Form.FormFields)
        {
            FormElementField field = fe.Fields[item.Name];
            field.Component = item.Component;
            field.VisibleExpression = item.VisibleExpression;
            field.EnableExpression = item.EnableExpression;
            field.TriggerExpression = item.TriggerExpression;
            field.Order = item.Order;
            field.LineGroup = item.LineGroup;
            field.CssClass = item.CssClass;
            field.HelpDescription = item.HelpDescription;
            field.DataItem = item.DataItem;
            field.MinValue = item.MinValue;
            field.MaxValue = item.MaxValue;
            field.DataFile = item.DataFile;
            field.Export = item.Export;
            field.ValidateRequest = item.ValidateRequest ?? true;
            field.AutoPostBack = item.AutoPostBack;
            field.NumberOfDecimalPlaces = item.NumberOfDecimalPlaces;
            field.Actions = item.Actions;
            field.Attributes = item.Attributes;
            field.PanelId = item.PanelId;
            field.InternalNotes = item.InternalNotes;
        }

        return fe;
    }

}