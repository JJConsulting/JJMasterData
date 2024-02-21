using System.Text;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

public abstract class ProcessComponent(
        IHttpContext currentContext,
        ExpressionsService expressionsService,
        FieldsService fieldsService,
        IBackgroundTaskManager backgroundTaskManager,
        ILogger<ProcessComponent> logger,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : AsyncComponent
{
    private string _processKey;
    private ProcessOptions _processOptions;
    private string _userId;

    internal ExpressionsService ExpressionsService { get; } = expressionsService;

    internal string ProcessKey
    {
        get
        {
            if (string.IsNullOrEmpty(_processKey))
                _processKey = BuildProcessKey();

            return _processKey;
        }
    }

    /// <summary>
    /// Id do usuário Atual
    /// </summary>
    /// <remarks>
    /// Se a variavel não for atribuida diretamente,
    /// o sistema tenta recuperar em UserValues ou nas variaveis de Sessão
    /// </remarks>
    internal string UserId => _userId ??= DataHelper.GetCurrentUserId(CurrentContext, UserValues);

    public IHttpContext CurrentContext { get; init; } = currentContext;

    public ProcessOptions ProcessOptions
    {
        get => _processOptions ??= new ProcessOptions();
        set => _processOptions = value;
    }

    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }
    
    internal FieldsService FieldsService { get; } = fieldsService;
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

        processKey.Append((string)FormElement.Name);

        if (ProcessOptions.Scope != ProcessScope.User)
            return processKey.ToString();

        if (string.IsNullOrEmpty(UserId))
        {
            var error = new StringBuilder();
            error.AppendLine("User not found, contact system administrator.");
            error.Append("Import configured with scope per user, but no key with USERID found.");

            var errorMessage = error.ToString();
            
            var exception = new JJMasterDataException(errorMessage);
            Logger.LogError(exception,"Error while creating process key");
            
            throw exception;
        }

        processKey.Append($"?userid={UserId}");

        return processKey.ToString();
    }

}
