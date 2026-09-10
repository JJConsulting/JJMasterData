#nullable disable warnings
using System.Collections.Generic;
using JJMasterData.Commons.Security;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

public abstract class ProcessComponent(
        IHttpContextAccessor httpContextAccessor,
        IMasterDataUser masterDataUser,
        ExpressionsService expressionsService,
        ILogger<ProcessComponent> logger,
        DataProtectionService dataProtectionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : AsyncComponent
{
    internal ExpressionsService ExpressionsService { get; } = expressionsService;

    public Dictionary<string, object?> UserValues { get; set; } = new();

    /// <summary>
    /// Id do usuário Atual
    /// </summary>
    /// <remarks>
    /// </remarks>
    internal string UserId => field ??= masterDataUser.Id;

    public IHttpContextAccessor HttpContextAccessor { get; init; } = httpContextAccessor;

    public ProcessOptions ProcessOptions
    {
        get => field ??= new ProcessOptions();
        set;
    }

    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }
    
    private ILogger<ProcessComponent> Logger { get; } = logger;
    internal DataProtectionService DataProtectionService { get; } = dataProtectionService;
    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

}
