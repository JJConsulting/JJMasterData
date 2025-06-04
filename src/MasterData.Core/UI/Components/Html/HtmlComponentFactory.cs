# nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class HtmlComponentFactory(
    ExpressionsService expressionsService,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IHttpContext currentContext,
    IServiceProvider serviceProvider)
{
    public AlertFactory Alert => new();

    public BreadcrumbFactory Breadcrumb => new();
    
    public CardFactory Card => new();
    
    public CollapsePanelFactory CollapsePanel => new(currentContext.Request.Form);
   
    public IconFactory Icon => new();
    
    public ImageFactory Image => new();
    
    public LabelFactory Label => new(stringLocalizer);
    
    public LinkButtonFactory LinkButton => serviceProvider.GetRequiredService<LinkButtonFactory>();
    
    public MessageBoxFactory MessageBox =>  new(stringLocalizer);
    
    public MessageToastFactory MessageToast =>  new(stringLocalizer);
    
    public OffcanvasFactory Offcanvas => new();
    
    public ModalDialogFactory ModalDialog => new();
    
    public SpinnerFactory Spinner => new();
    
    public NavFactory TabNav => new(currentContext.Request.Form);
    
    public TitleFactory Title => new(expressionsService);
    
    public ToolbarFactory Toolbar => new();
    
    public ValidationSummaryFactory ValidationSummary =>  new(stringLocalizer);
    public LinkButtonGroupFactory LinkButtonGroup => new(stringLocalizer);
}

