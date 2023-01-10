using System.Text;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Dao.Entity.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Facades;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.WebComponents;

public abstract class JJBaseProcess : JJBaseView
{
    private string _processKey;
    private ProcessOptions _processOptions;
    private FieldManager _fieldManager;
    private FormManager _formManager;
    private ExpressionManager _expressionManager;
    private string _userId;
    
    public string UserId => _userId ??= DataHelper.GetCurrentUserId(HttpContext, UserValues);
    

    private readonly RepositoryServicesFacade _repositoryServicesFacade;
    
    private ILogger<JJBaseProcess> Logger { get; }
    
    internal ILoggerFactory LoggerFactory { get; }
    
    protected JJBaseProcess(IHttpContext httpContext,
        RepositoryServicesFacade repositoryServicesFacade,
        IBackgroundTask backgroundTask,
        JJMasterDataEncryptionService encryptionService,
        IOptions<JJMasterDataCoreOptions> options,
        ILoggerFactory loggerFactory)
    {
        DataDictionaryRepository = repositoryServicesFacade.DataDictionaryRepository;
        EntityRepository = repositoryServicesFacade.EntityRepository;
        HttpContext = httpContext;
        _repositoryServicesFacade = repositoryServicesFacade;
        LoggerFactory = loggerFactory;
        BackgroundTask = backgroundTask;
        EncryptionService = encryptionService;
        Options = options;
        Logger = LoggerFactory.CreateLogger<JJBaseProcess>();
    }

    internal ExpressionManager ExpressionManager => _expressionManager ??= new ExpressionManager(UserValues, EntityRepository, HttpContext, LoggerFactory);

    public IDataDictionaryRepository DataDictionaryRepository { get; }
    internal IEntityRepository EntityRepository { get; }
    
    internal IHttpContext HttpContext { get; }

    internal string ProcessKey
    {
        get
        {
            if (string.IsNullOrEmpty(_processKey))
                _processKey = BuildProcessKey();

            return _processKey;
        }
    }

    public ProcessOptions ProcessOptions
    {
        get => _processOptions ??= new ProcessOptions();
        set => _processOptions = value;
    }

    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }


    internal FieldManager FieldManager =>
        _fieldManager ??= new FieldManager(FormElement,HttpContext, _repositoryServicesFacade,ExpressionManager,EncryptionService,Options,LoggerFactory);

    internal FormManager FormManager => _formManager ??= new FormManager(FormElement, ExpressionManager);

    internal IBackgroundTask BackgroundTask { get; }
    public JJMasterDataEncryptionService EncryptionService { get; }
    public IOptions<JJMasterDataCoreOptions> Options { get; }

    internal bool IsRunning()
    {
        return BackgroundTask.IsRunning(ProcessKey);
    }

    internal void AbortProcess()
    {
        BackgroundTask.Abort(ProcessKey);
    }


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

        if (string.IsNullOrEmpty(DataHelper.GetCurrentUserId(HttpContext, UserValues)))
        {
            var error = new StringBuilder();
            error.AppendLine(Translate.Key("User not found, contact system administrator."));
            error.Append(Translate.Key("Import configured with scope per user, but no key with USERID found."));

            var exception = new JJMasterDataException(error.ToString());
            Logger.LogError(exception, "User not found.");

            throw exception;
        }

        processKey.Append($"?userid={UserId}");

        return processKey.ToString();
    }
}