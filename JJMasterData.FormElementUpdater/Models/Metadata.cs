#nullable disable

using System.Collections;
using System.Runtime.Serialization;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.FormElementUpdater.Models;

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
        var formElement = new FormElement(Table)
        {
            Title = Form.Title,
            SubTitle = Form.SubTitle,
            Panels = Form.Panels,
            Info = Table.Info,
            Indexes = Table.Indexes,
            Sync = Table.Sync,
            SyncMode = Table.SyncMode,
        };

        foreach (var item in Form.FormFields)
        {
            var field = formElement.Fields[item.Name];
            field.Component = item.Component;
            field.VisibleExpression = item.VisibleExpression;
            field.EnableExpression = item.EnableExpression;
            field.TriggerExpression = item.TriggerExpression;
            field.Order = item.Order;
            field.LineGroup = item.LineGroup;
            field.CssClass = item.CssClass;
            field.HelpDescription = item.HelpDescription;
            field.DataItem = item.DataItem;
            field.Attributes[FormElementField.MinValueAttribute] = item.MinValue;
            field.Attributes[FormElementField.MaxValueAttribute] = item.MaxValue;
            field.DataFile = item.DataFile;
            field.Export = item.Export;
            field.ValidateRequest = item.ValidateRequest ?? true;
            field.AutoPostBack = item.AutoPostBack;
            field.NumberOfDecimalPlaces = item.NumberOfDecimalPlaces;
            field.Actions = item.Actions;
            field.Attributes = item.Attributes?.Cast<DictionaryEntry>()
                .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value) ?? new Dictionary<string, dynamic>();
            field.PanelId = item.PanelId;
            field.InternalNotes = item.InternalNotes;
        }


        formElement.Options = new FormElementOptions
        {
            Form = Options.Form,
            Grid = Options.Grid,
            GridActions = Options.GridActions,
            ToolbarActions = Options.ToolbarActions
        };

        formElement.ApiOptions = new FormElementApiOptions
        {
            FormatType = ApiOptions.FormatType,
            EnableAdd = ApiOptions.EnableAdd,
            EnableGetAll = ApiOptions.EnableGetAll,
            EnableDel = ApiOptions.EnableDel,
            EnableUpdate = ApiOptions.EnableUpdate,
            EnableGetDetail = ApiOptions.EnableGetDetail,
            EnableUpdatePart = ApiOptions.EnableUpdatePart,
            ApplyUserIdOn = ApiOptions.ApplyUserIdOn
        };

        return formElement;
    }
    
}