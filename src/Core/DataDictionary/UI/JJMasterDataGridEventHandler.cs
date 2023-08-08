using System.Threading.Tasks;
using JJMasterData.Commons.Data.Extensions;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Options;
using JJMasterData.Core.UI.FormEvents.Abstractions;
using JJMasterData.Core.Web;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Factories;

public class JJMasterDataGridEventHandler : GridEventHandlerBase
{
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private JJMasterDataCoreOptions Options { get; }

    public JJMasterDataGridEventHandler(IDataDictionaryRepository dataDictionaryRepository, JJMasterDataUrlHelper urlHelper, IOptions<JJMasterDataCoreOptions> options)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        UrlHelper = urlHelper;
        Options = options.Value;
    }
    
    public override string ElementName => Options.DataDictionaryTableName;
    
    public override async Task OnDataLoadAsync(object sender, GridDataLoadEventArgs eventArgs)
    {
        var filter = DataDictionaryFilter.GetInstance(eventArgs.Filters);
        string orderBy = string.IsNullOrEmpty(eventArgs.OrderBy) ? "name ASC" : eventArgs.OrderBy;
        var result = await DataDictionaryRepository.GetFormElementInfoListAsync(filter, orderBy, eventArgs.RegporPag, eventArgs.CurrentPage);
        eventArgs.DataSource = result.Data.ToDataTable();
        eventArgs.Tot = result.TotalOfRecords;
    }

    public override void OnRenderAction(object sender, ActionEventArgs eventArgs)
    {
        var formName = eventArgs.FieldValues["name"]?.ToString();
        switch (eventArgs.Action.Name)
        {
            case "preview":
                eventArgs.LinkButton.OnClientClick =
                    $"window.open('{UrlHelper.GetUrl("Render", "Form", "MasterData",new { dictionaryName = formName })}', '_blank').focus();";
                break;
            case "tools":
                eventArgs.LinkButton.UrlAction = UrlHelper.GetUrl("Index", "Entity", "MasterData",new { dictionaryName = formName });
                eventArgs.LinkButton.OnClientClick = "";
                break;
            case "duplicate":
                eventArgs.LinkButton.UrlAction = UrlHelper.GetUrl("Duplicate", "Element", "MasterData",new { dictionaryName = formName });
                eventArgs.LinkButton.OnClientClick = "";
                break;
        }
    }

    public override async Task OnRenderHtmlAsync(object sender, GridRenderEventArgs eventArgs)
    {
        var filter = await eventArgs.GridView.GetCurrentFilterAsync();
        filter["type"] = "F";
        
        eventArgs.GridView.FilterAction.ExpandedByDefault = true;
    }
}