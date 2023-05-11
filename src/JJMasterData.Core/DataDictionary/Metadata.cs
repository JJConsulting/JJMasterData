using System;
using System.Runtime.Serialization;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;

namespace JJMasterData.Core.DataDictionary;

[DataContract(Name = "elementInfo")]
public class Metadata
{
    [DataMember(Name = "table")]
    public Element Table { get; set; }

    [DataMember(Name = "form")]
    public MetadataForm Form { get; set; }

    [DataMember(Name = "uioptions")]
    public MetadataOptions Options { get; set; }

    [DataMember(Name = "api")]
    public MetadataApiOptions ApiOptions { get; set; }

    public static explicit operator FormElement(Metadata metadata) => metadata.GetFormElement();

    public FormElement GetFormElement()
    {
        if (Table == null)
            return null;

        if (Form == null)
            return null;

        var fe = new FormElement(Table)
        {
            Title = Form.Title,
            SubTitle = Form.SubTitle,
            Panels = Form.Panels
        };

        fe.Relationships = Form.Relationships;

        foreach (var item in Form.FormFields)
        {
            var field = fe.Fields[item.Name];
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

    public void SetFormElement(FormElement formElement)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        if (string.IsNullOrEmpty(formElement.Name))
            throw new ArgumentException(Translate.Key("Invalid dictionary name"));

        for (var i = 0; i < formElement.Fields.Count; i++)
        {
            formElement.Fields[i].Order = i + 1;
        }

        Table = formElement.DeepCopy<Element>();
        Form = new MetadataForm(formElement);
    }
}