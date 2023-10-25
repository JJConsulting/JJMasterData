using System.Text;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
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

public abstract class ProcessComponent : AsyncComponent
{
    private string _processKey;
    private ProcessOptions _processOptions;
    private string _userId;

    internal ExpressionsService ExpressionsService { get; }

    internal IEntityRepository EntityRepository { get; }

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

    public IHttpContext CurrentContext { get; init; }

    public ProcessOptions ProcessOptions
    {
        get => _processOptions ??= new ProcessOptions();
        set => _processOptions = value;
    }

    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }
    
    internal FieldsService FieldsService { get; } 
    internal IBackgroundTaskManager BackgroundTaskManager { get; }
    private ILogger<ProcessComponent> Logger { get; }
    internal IEncryptionService EncryptionService { get; }
    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; }

    protected ProcessComponent(
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        ExpressionsService expressionsService,
        FieldsService fieldsService,
        IBackgroundTaskManager backgroundTaskManager,
        ILogger<ProcessComponent> logger,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    {
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
        FieldsService = fieldsService;
        BackgroundTaskManager = backgroundTaskManager;
        Logger = logger;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
        CurrentContext = currentContext;
    }
    
    internal bool IsRunning() => BackgroundTaskManager.IsRunning(ProcessKey);

    internal void StopExportation() => BackgroundTaskManager.Abort(ProcessKey);
    
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
