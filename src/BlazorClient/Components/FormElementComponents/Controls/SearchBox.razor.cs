using JJMasterData.BlazorClient.Services;
using JJMasterData.Core.DataDictionary;
using Microsoft.AspNetCore.Components;

namespace JJMasterData.BlazorClient.Components.FormElementComponents.Controls;

public partial class SearchBox : ComponentBase
{
    [Inject]
    private ISearchBoxService SearchBoxService { get; set; } = null!;
    
    [Parameter]
    public FormElementDataItem DataItem { get; set; } = null!;
    
    [Parameter]
    public SearchBoxContext Context { get; set; } = null!;

    [Parameter]
    public string? SelectedValue { get; set; }

    [Parameter]
    public IEnumerable<DataItemValue>? Values { get; set; }
    
    [Parameter]
    public bool Disabled { get; set; }
    
    [Parameter]
    public Action<string>? OnSelectValue { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        if (SelectedValue != null)
        {
            var selectedDataItemValue = await GetSelectedValue(SelectedValue);

            if (selectedDataItemValue != null)
            {
                selectedValueId = selectedDataItemValue.Id;
                selectedDescription = selectedDataItemValue.Description;
            }
        }

        await base.OnInitializedAsync();
    }
    
    private async Task<DataItemValue?> GetSelectedValue(string? id)
    {
        Values ??= await SearchBoxService.GetValues(DataItem, null, Context);

        return Values.FirstOrDefault(v => v.Id == id);
    }
}