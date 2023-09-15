# nullable enable

using JJMasterData.Commons.Localization;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

public class HtmlComponentFactory
{
    private readonly IStringLocalizer<JJMasterDataResources> _stringLocalizer;
    private readonly IHttpContext _currentContext;

    public HtmlComponentFactory(
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IHttpContext currentContext)
    {
        _stringLocalizer = stringLocalizer;
        _currentContext = currentContext;
    }
    
    public AlertFactory Alert => new();

    public CardFactory Card => new();
    
    public CollapsePanelFactory CollapsePanel => new(_currentContext);
    
    public IconFactory Icon => new();
    
    public ImageFactory Image =>  new(_currentContext);
    
    public LabelFactory Label => new(_stringLocalizer);
    
    public MessageFactory MessageBox =>  new(_stringLocalizer);
    
    public ModalDialogFactory ModalDialog => new();
    
    public SpinnerFactory Spinner => new();
    
    public NavFactory TabNav => new(_currentContext);
    
    public TitleFactory Title => new();
    
    public ToolbarFactory Toolbar => new();
    
    public ValidationSummaryFactory ValidationSummary =>  new(_stringLocalizer);
    
}

