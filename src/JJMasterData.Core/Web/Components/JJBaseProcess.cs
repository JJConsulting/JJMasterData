using System.Text;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Core.Web.Components;

public abstract class JJBaseProcess : JJBaseView
{
    private string _keyProcess;
    private ProcessOptions _processOptions;
    private FieldManager _fieldManager;
    private FormManager _formManager;
    private ExpressionManager _expressionManager;
    IEntityRepository _entityRepository;

    internal ExpressionManager ExpressionManager
    {
        get
        {
            if (_expressionManager == null)
                _expressionManager = new ExpressionManager(UserValues, EntityRepository);

            return _expressionManager;
        }
    }

    internal IEntityRepository EntityRepository
    {
        get
        {
            if (_entityRepository == null)
                _entityRepository = JJService.EntityRepository;

            return _entityRepository;
        }
        set
        {
            _entityRepository = value;
        }
    }

    internal string ProcessKey
    {
        get
        {
            if (string.IsNullOrEmpty(_keyProcess))
                _keyProcess = BuildProcessKey();

            return _keyProcess;
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


    internal FieldManager FieldManager
    {
        get
        {
            if (_fieldManager == null)
                _fieldManager = new FieldManager(FormElement, ExpressionManager);

            return _fieldManager;
        }
    }

    internal FormManager FormManager
    {
        get
        {
            if (_formManager == null)
                _formManager = new FormManager(FormElement, ExpressionManager);

            return _formManager;
        }
    }

    internal IBackgroundTask BackgroundTask => JJService.BackgroundTask;

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
