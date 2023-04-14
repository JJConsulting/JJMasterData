using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class LogViewModel
{
    public FormElement FormElement { get; set; }
    public Action<JJGridView> Configuration { get; set; }
    
    public LogViewModel(FormElement formElement, Action<JJGridView> configuration)
    {
        FormElement = formElement;
        Configuration = configuration;
    }
}