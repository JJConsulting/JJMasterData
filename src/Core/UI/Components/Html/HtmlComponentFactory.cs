# nullable enable

using System;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class HtmlComponentFactory
{
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer;
    private readonly IHttpContext _currentContext;
    private readonly MasterDataUrlHelper _urlHelper;
    private readonly IServiceProvider _serviceProvider;

    public HtmlComponentFactory(
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IHttpContext currentContext,
        MasterDataUrlHelper urlHelper,
        IServiceProvider serviceProvider)
    {
        _stringLocalizer = stringLocalizer;
        _currentContext = currentContext;
        _urlHelper = urlHelper;
        _serviceProvider = serviceProvider;
    }
    
    public AlertFactory Alert => new();

    public CardFactory Card => new();
    
    public CollapsePanelFactory CollapsePanel => new(_currentContext.Request.Form);
    
    public IconFactory Icon => new();
    
    public ImageFactory Image =>  new(_currentContext);
    
    public LabelFactory Label => new(_stringLocalizer);
    
    public LinkButtonFactory LinkButton => _serviceProvider.GetRequiredService<LinkButtonFactory>();
    
    public MessageBoxFactory MessageBox =>  new(_stringLocalizer);
    
    public ModalDialogFactory ModalDialog => new();
    
    public SpinnerFactory Spinner => new();
    
    public NavFactory TabNav => new(_currentContext.Request.Form);
    
    public TitleFactory Title => new();
    
    public ToolbarFactory Toolbar => new();
    
    public ValidationSummaryFactory ValidationSummary =>  new(_stringLocalizer);
    
}

