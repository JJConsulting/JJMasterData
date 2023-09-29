# nullable enable

using System;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.Widgets;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

public class HtmlComponentFactory
{
    private readonly IStringLocalizer<JJMasterDataResources> _stringLocalizer;
    private readonly IHttpContext _currentContext;
    private readonly IServiceProvider _serviceProvider;

    public HtmlComponentFactory(
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IHttpContext currentContext,
        IServiceProvider serviceProvider)
    {
        _stringLocalizer = stringLocalizer;
        _currentContext = currentContext;
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

