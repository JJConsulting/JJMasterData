#nullable disable

using System.Collections;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using Newtonsoft.Json;

namespace JJMasterData.ConsoleApp.Models.FormElementMigration;

[JsonObject("elementInfo")]
public class Metadata
{
    [JsonProperty("table")]
    public Element Table { get; set; }

    [JsonProperty("form")]
    public MetadataForm Form { get; set; }

    [JsonProperty("uioptions")]
    public MetadataOptions Options { get; set; }

    [JsonProperty("api")]
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
            EnableApi = Table.EnableApi,
            SyncMode = Table.SyncMode,
        };

        foreach (var item in Form.FormFields)
        {
            var field = formElement.Fields[item.Name];
            field.Component = item.Component;
            field.VisibleExpression = item.VisibleExpression;
            field.EnableExpression = item.EnableExpression;
            field.TriggerExpression = item.TriggerExpression;
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
                .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value) ?? new Dictionary<string, object>();
            field.PanelId = item.PanelId;
            field.InternalNotes = item.InternalNotes;
        }


        formElement.Options = new FormElementOptions
        {
            Form = Options.Form,
            Grid = Options.Grid
        };

        foreach (var action in Options.GridActions.GetAll())
        {
            formElement.Options.GridTableActions.Add(action);
        }
        
        foreach (var action in Options.ToolbarActions.GetAll())
        {
            formElement.Options.GridTableActions.Add(action);
        }

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