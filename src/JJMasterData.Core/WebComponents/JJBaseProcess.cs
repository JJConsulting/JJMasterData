using System;
using System.Text;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Core.WebComponents;

public abstract class JJBaseProcess : JJBaseView
{
    private FieldManager _fieldManager;
    private FormManager _formManager;
    private ProcessOptions _processOptions;
    private string _keyProcess;

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

   

    /// <summary>
    /// Funções úteis para manipular campos no formulário
    /// </summary>
    internal FieldManager FieldManager => _fieldManager ??= new FieldManager(this, FormElement);

    internal FormManager FormManager => _formManager ??= new FormManager(FormElement, UserValues, DataAccess);

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
            Log.AddError(error.ToString());
            throw new Exception(error.ToString());
        }

        processKey.Append($"?userid={UserId}");

        return processKey.ToString();
    }

}
