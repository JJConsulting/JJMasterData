#nullable disable warnings
using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Web.Components;

public abstract class ProcessComponent(
        IHttpContextAccessor httpContextAccessor,
        IMasterDataUser masterDataUser,
        ExpressionsService expressionsService,
        IBackgroundTaskManager backgroundTaskManager,
        ILogger<ProcessComponent> logger,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : AsyncComponent
{
    internal ExpressionsService ExpressionsService { get; } = expressionsService;

    internal string ProcessKey
    {
        get
        {
            if (string.IsNullOrEmpty(field))
                field = BuildProcessKey();

            return field;
        }
    }

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
    
    internal IBackgroundTaskManager BackgroundTaskManager { get; } = backgroundTaskManager;
    private ILogger<ProcessComponent> Logger { get; } = logger;
    internal IEncryptionService EncryptionService { get; } = encryptionService;
    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    internal bool IsRunning() => BackgroundTaskManager.IsRunning(ProcessKey);

    internal void StopImportation() => BackgroundTaskManager.Abort(ProcessKey);
    
    private string BuildProcessKey()
    {
        var processKey = new StringBuilder();

        switch (this)
        {
            case JJDataExportation:
                processKey.Append("Export/");
                break;
            case JJDataImportation:
                processKey.Append("Import/");
                break;
        }

        processKey.Append(FormElement.Name);

        if (ProcessOptions.Scope != ProcessScope.User)
            return processKey.ToString();

        if (string.IsNullOrEmpty(UserId))
            return processKey.ToString();

        processKey.Append($"?userid={UserId}");

        return processKey.ToString();
    }

}
