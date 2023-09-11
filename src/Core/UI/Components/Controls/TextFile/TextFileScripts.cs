#nullable enable

using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Components;
using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components.Importation;

internal class TextFileScripts
{
    public JJTextFile TextFile { get; }

    public TextFileScripts(JJTextFile textFile)
    {
        TextFile = textFile;
    }

    public string GetShowScript()
    {
        var parms = new UploadViewParams
        {
            PageState = TextFile.PageState,
            Enable = TextFile.Enabled && !TextFile.ReadOnly
        };

        if (TextFile.PageState != PageState.Insert)
            parms.PkValues = DataHelper.ParsePkValues(TextFile.FormElement, TextFile.FormValues, '|');

        var json = JsonConvert.SerializeObject(parms);
        var values = TextFile.EncryptionService.EncryptStringWithUrlEscape(json);

        var title = TextFile.FormElementField.Label;
        title = title == null ? "Manage Files" : title.Replace('\'', '`').Replace('\"', ' ');

        title = TextFile.StringLocalizer[title];

        return $"UploadViewHelper.show('{TextFile.Name}','{title}','{values}');";
    }

}