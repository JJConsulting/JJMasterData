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
                _keyProcess = BuildKeyProcess();

            return _keyProcess;
        }
    }

    public ProcessOptions ProcessOptions
    {
        get
        {
            if (_processOptions == null)
                _processOptions = new ProcessOptions();
            return _processOptions;
        }
        set
        {
            _processOptions = value;
        }
    }

    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }

   

    /// <summary>
    /// Funções úteis para manipular campos no formulário
    /// </summary>
    internal FieldManager FieldManager
    {
        get
        {
            if (_fieldManager == null)
                _fieldManager = new FieldManager(this, FormElement);

            return _fieldManager;
        }
    }

    internal FormManager FormManager
    {
        get
        {
            if (_formManager == null)
                _formManager = new FormManager(FormElement, UserValues, DataAccess);

            return _formManager;
        }
    }

    internal IBackgroundTask BackgroundTask
    {
        get
        {
            return JJService.BackgroundTask;
        }
    }
        
    internal bool IsRunning()
    {
        return BackgroundTask.IsRunning(ProcessKey);
    }

    internal void AbortProcess()
    {
        BackgroundTask.Abort(ProcessKey);
    }


    private string BuildKeyProcess()
    {
        var keyprocess = new StringBuilder();
        if (this is JJDataExp)
            keyprocess.Append("Export/");
        else if (this is JJDataImp)
            keyprocess.Append("Import/");

        keyprocess.Append(FormElement.Name);
        if (ProcessOptions.Scope == ProcessScope.User)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                var error = new StringBuilder();
                error.AppendLine(Translate.Key("User not found, contact system administrator."));
                error.Append(Translate.Key("Import configured with scope per user, but no key with USERID found."));
                Log.AddError(error.ToString());
                throw new Exception(error.ToString());
            }
            keyprocess.AppendFormat("?userid={0}", UserId);
        }
        
        return keyprocess.ToString();
    }

}
