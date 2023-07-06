using System.Text;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Components;

public abstract class JJBaseProcess : JJBaseView
{
    private string _processKey;
    private ProcessOptions _processOptions;
    private string _userId;

    internal IExpressionsService ExpressionsService { get; }

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

    public IHttpContext CurrentContext { get; }

    public ProcessOptions ProcessOptions
    {
        get => _processOptions ??= new ProcessOptions();
        set => _processOptions = value;
    }

    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }
    
    internal IFieldValuesService FieldValuesService { get; } 
    
    internal IBackgroundTask BackgroundTask { get; }
    private ILogger<JJBaseProcess> Logger { get; }
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    protected JJBaseProcess(IHttpContext currentContext, IEntityRepository entityRepository,
        IExpressionsService expressionsService,
        IFieldValuesService fieldValuesService,
        IBackgroundTask backgroundTask,
        ILogger<JJBaseProcess> logger,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
        FieldValuesService = fieldValuesService;
        BackgroundTask = backgroundTask;
        Logger = logger;
        StringLocalizer = stringLocalizer;
        CurrentContext = currentContext;
    }
    
    internal bool IsRunning() => BackgroundTask.IsRunning(ProcessKey);

    internal void StopExportation() => BackgroundTask.Abort(ProcessKey);
    
    private string BuildProcessKey()
    {
        var processKey = new StringBuilder();

        switch (this)
        {
            case JJDataExp:
                processKey.Append("Export/");
                break;
            case JJDataImp:
                processKey.Append("Import/");
                break;
        }

        processKey.Append(FormElement.Name);

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
