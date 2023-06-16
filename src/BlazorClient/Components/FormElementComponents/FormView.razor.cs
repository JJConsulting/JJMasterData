using System.ComponentModel.DataAnnotations;
using JJMasterData.BlazorClient.Components.BootstrapComponents;
using JJMasterData.BlazorClient.Services.Abstractions;
using JJMasterData.Commons.Data.Entity.Services;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace JJMasterData.BlazorClient.Components.FormElementComponents;

public partial class FormView : ComponentBase
{
    [Parameter] public IDictionary<string, dynamic?>? PrimaryKeys { get; set; }

    [Parameter] public IDictionary<string, dynamic?>? Values { get; set; }

    [Parameter] public IEnumerable<ValidationResult>? Errors { get; set; }

    [Inject] public IDataDictionaryService DataDictionaryService { get; set; } = null!;
    [Inject] public IExpressionsService ExpressionsService { get; set; } = null!;
    [Inject] public IFieldsService FieldsService { get; set; } = null!;

    [Inject] public IHttpContextAccessor HttpContextAccessor { get; set; } = null!;

    [Inject] public IEntityService EntityService { get; set; } = null!;

    [Parameter] public string ElementName { get; set; } = default!;

    [Parameter] public FormElement? FormElement { get; set; }

    [Parameter] public PageState PageState { get; set; } = PageState.List;

    [Parameter] public IDictionary<string, dynamic?>? UserValues { get; set; }

    [CascadingParameter] public MasterDataView? MasterDataView { get; set; }
    [CascadingParameter] public Modal? Modal { get; set; }
    
    [CascadingParameter] public GridView? ParentGridView { get; set; }
    
    private IList<FormElementField>? VisibleFields { get; set; }
    
    protected override async Task OnParametersSetAsync()
    {
        if (Values.IsNullOrEmpty())
            await PopulateValuesAsync();
        
        VisibleFields = await FieldsService.GetVisibleFields(FormElement!.Fields, PageState, Values).ToListAsync();

        await base.OnParametersSetAsync();
    }

    private async Task PopulateValuesAsync(int currentPage = 1, int recordsPerPage = 5)
    {
        FormElement ??= await DataDictionaryService.GetAsync(ElementName);

        if (FormElement != null && PageState is PageState.Update or PageState.View)
        {
            if (PrimaryKeys != null)
            {
                Values ??= await EntityService.GetDictionaryAsync(FormElement, PrimaryKeys!);
            }
        }
        else
        {
            Values ??= new Dictionary<string, dynamic?>();
        }
    }

    internal async Task OnSave()
    {
        Errors = FieldsService.ValidateFields(FormElement!, Values!, PageState, true);

        if (!Errors.Any())
        {
            switch (PageState)
            {
                case PageState.Update:
                    await EntityService.UpdateAsync(FormElement!, Values!);
                    MasterDataView?.SetState(PageState.List);
                    break;
                case PageState.Insert:
                {
                    await EntityService.InsertAsync(FormElement!, Values!);

                    var insertAction = FormElement!.Options.GridToolbarActions.First(a => a is InsertAction) as InsertAction;
                    
                    if (insertAction!.ReopenForm)
                    {
                        MasterDataView?.SetState(PageState.Insert);
                    }
                    else
                    {
                        if (Modal is not null)
                        {
                            await Modal.DismissAsync();
                        }
                        MasterDataView?.SetState(PageState.List);
                    }
                    break;
                }
            }
        }
        else
        {
            StateHasChanged();
        }
    }
}