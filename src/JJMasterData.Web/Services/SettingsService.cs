using System.Collections;
using System.Reflection;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Settings;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Web.Services;

public class SettingsService
{
    public ISettings Settings { get; }

    public SettingsService(ISettings settings)
    {
        Settings = settings;
    }

    public JJDataPanel GetDataPanel()
    {
        var formElement = GetFormElement();

        var dataPanel = new JJDataPanel(formElement)
        {
            PageState = PageState.Update,
            Name = "ISettings",
            Values = GetValues()
        };

        return dataPanel;
    }

    private Hashtable GetValues()
    {
        var hashtable = new Hashtable();
        foreach (var field in Settings.GetType().GetRuntimeProperties())
        {
            hashtable[field.Name] = field.GetValue(Settings);
        }

        return hashtable;
    }

    private FormElement GetFormElement()
    {
        var formElement = new FormElement
        {
            Name = "ISettings",
            Title = Translate.Key("JJMasterData Settings"),
        };

        foreach (var field in GetFormElementFields())
        {
            formElement.Fields.Add(field);
        }

        return formElement;
    }

    private IEnumerable<FormElementField> GetFormElementFields()
    {
        var fields = new List<FormElementField>();
        foreach (var field in Settings.GetType().GetRuntimeProperties().Where(p=>!p.GetAccessors(true)[0].IsStatic))
        {
            fields.Add(new FormElementField()
            {
                Name = field.Name, CssClass = "col-sm-3",
                IsPk = field.GetValue(Settings) != null,
                DataType = field.PropertyType == typeof(string) ? FieldType.Varchar : FieldType.Int
            });
        }

        return fields;
    }
}