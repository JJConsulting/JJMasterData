using System.Collections;
using JJMasterData.BlazorClient.Services.Abstractions;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using Microsoft.AspNetCore.Components;

namespace JJMasterData.BlazorClient.Components.FormElementComponents;

public partial class MasterDataView : ComponentBase
{
    public string Id { get; } = Guid.NewGuid().ToString();
    
    [Inject]
    public IDataDictionaryService DataDictionaryService { get; set; } = null!;

    [Parameter]
    public string ElementName { get; set; } = null!;
    
    [Parameter]
    public FormElement? FormElement { get; set; } = default!;
    
    [Parameter]
    public IEnumerable<FormElement> ChildFormElements { get; set; } = default!;
    
    [Parameter]
    public PageState PageState { get; set; } = PageState.List;
    
    [Parameter]
    public IDictionary<string,dynamic>? SelectedPrimaryKeys { get; set; }

    public void SetState(PageState pageState, IDictionary<string,dynamic>? selectedPrimaryKeys = null)
    {
        PageState = pageState;
        SelectedPrimaryKeys = selectedPrimaryKeys;
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            FormElement ??= await DataDictionaryService.GetAsync(ElementName);

            ChildFormElements = await GetChildFormElements().ToListAsync();
            
            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async IAsyncEnumerable<FormElement> GetChildFormElements()
    {
        foreach (var relationship in FormElement!.Relationships.GetElementRelationships())
        {
            yield return (await DataDictionaryService.GetAsync(relationship.ChildElement))!;
        }
    }
    
    private IDictionary<string, dynamic?> GetMappedForeignKeys(FormElement formElement, IDictionary<string, dynamic?> values)
    {
        var foreignKeys = new Dictionary<string, dynamic?>();
        var relationships = formElement.Relationships.GetElementRelationships();

        foreach (var entry in values)
        {
            var matchingRelationship = relationships.FirstOrDefault(r => r.Columns.Any(c => c.PkColumn == entry.Key));

            if (matchingRelationship != null)
            {
                var matchingColumn = matchingRelationship.Columns.First(c => c.PkColumn == entry.Key);
                foreignKeys[matchingColumn.FkColumn] = entry.Value;
            }
        }

        return foreignKeys;
    }

}