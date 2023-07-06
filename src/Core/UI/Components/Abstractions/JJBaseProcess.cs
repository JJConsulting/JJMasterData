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
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

public abstract class JJBaseProcess : JJBaseView
{
    private string _processKey;
    private ProcessOptions _processOptions;


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
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    protected JJBaseProcess(
        IEntityRepository entityRepository,
        IExpressionsService expressionsService, 
        IFieldValuesService fieldValuesService,
        IBackgroundTask backgroundTask,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
        FieldValuesService = fieldValuesService;
        BackgroundTask = backgroundTask;
        StringLocalizer = stringLocalizer;
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
            error.AppendLine(Translate.Key("User not found, contact system administrator."));
            error.Append(Translate.Key("Import configured with scope per user, but no key with USERID found."));
            
            var exception = new JJMasterDataException(error.ToString());
            Log.AddError(exception, exception.Message);
            
            throw exception;
        }

        processKey.Append($"?userid={UserId}");

        return processKey.ToString();
    }

}
