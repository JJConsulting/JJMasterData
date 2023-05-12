#nullable disable

using System.Runtime.Serialization;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
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


        formElement.Options = new FormElementOptions
        {
            Form = Options.Form,
            Grid = Options.Grid,
            GridActions = Options.GridActions,
            ToolBarActions = Options.ToolBarActions
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