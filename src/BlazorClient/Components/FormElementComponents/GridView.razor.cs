using JJMasterData.BlazorClient.Extensions;
using JJMasterData.BlazorClient.Models;
using JJMasterData.BlazorClient.Services.Abstractions;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Services;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using Microsoft.AspNetCore.Components;

namespace JJMasterData.BlazorClient.Components.FormElementComponents;

public partial class GridView : ComponentBase
{
    private TableViewPaginationData? paginationData;
    
    [Parameter]
    public Action<BasicAction, IDictionary<string,dynamic?>>? OnActionRender { get; set; } 
    
    [Parameter]
    public IDictionary<string,dynamic>? SelectedPrimaryKeys { get; set; } 
    
    [Parameter] 
    public string ElementName { get; set; } = null!;
    
    [Parameter] 
    public FormElement? FormElement { get; set; }

    [Parameter]
    public DataSource? DataSource { get; set; } 
    
    public OrderByData? OrderByData { get; set; }
    
    [Inject] 
    public IDataDictionaryService DataDictionaryService { get; set; } = null!;
    
    [Inject] 
    public IEntityService EntityService { get; set; } = null!;


    public TableViewPaginationData? PaginationData
    {
        get
        {
            if (FormElement != null && DataSource != null)
            {
                paginationData ??= new TableViewPaginationData(
                    DataSource.TotalOfRecords, FormElement.Options.Grid.TotalPerPage, FormElement.Options.Grid.TotalPaggingButton);
            }

            return paginationData;
        }
        set => paginationData = value;
    }

    private IList<FormElementField>? VisibleFields { get; set; }

    public record TableViewPaginationData(int TotalOfRecords, int TotalPerPage, int TotalButtons)
    {
        public int PageCount => (int)Math.Ceiling(TotalOfRecords / (double)TotalPerPage);
        public int FirstButton => (int)Math.Floor((CurrentPage - 1) / (double)TotalButtons) * TotalButtons + 1;
        public int LastButton => FirstButton + TotalButtons;
        public int CurrentPage { get; set; } = 1;
        public bool IsPreviousButtonVisible() => FirstButton > TotalButtons;
        public bool IsNextButtonVisible() => LastButton <= PageCount;
    }
    
    protected override async Task OnAfterRenderAsync(bool isFirstRender)
    {
        if (isFirstRender)
        {
            VisibleFields = await FieldsService.GetVisibleFields(FormElement!.Fields, PageState.List).ToListAsync();
        
            await PopulateDataSourceAsync();
            
            StateHasChanged();
        }
        await base.OnAfterRenderAsync(isFirstRender);
    }

    public async Task RePopulateDataSourceAsync()
    {
        if (PaginationData != null) 
            PaginationData.CurrentPage = 1;
        await PopulateDataSourceAsync();
        StateHasChanged();
    }

    private async Task PopulateDataSourceAsync()
    {
        FormElement ??= await DataDictionaryService.GetAsync(ElementName);

        if (FormElement != null)
        {
            DataSource = await EntityService.GetEntityResultAsync(FormElement, new EntityParameters
            {
                PaginationData = new PaginationData(PaginationData?.CurrentPage ?? 1, PaginationData?.TotalPerPage ?? 5),
                OrderBy = OrderByData
            });
        }
    }
}

