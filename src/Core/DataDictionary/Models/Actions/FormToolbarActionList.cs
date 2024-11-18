using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class FormToolbarActionList : FormElementActionList
{
    private SaveAction _saveAction = new();
    private BackAction _backAction = new();
    private CancelAction _cancelAction = new();
    private FormEditAction _formEditAction = new();
    private AuditLogFormToolbarAction _auditLogFormToolbarAction = new();

    [JsonPropertyName("saveAction")]
    public SaveAction SaveAction
    {
        get => (SaveAction)List.Find(a => a is SaveAction) ?? _saveAction;
        set
        {
            _saveAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("backAction")]
    public BackAction BackAction
    {
        get => (BackAction)List.Find(a => a is BackAction) ?? _backAction;
        set
        {
            _backAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("cancelAction")]
    public CancelAction CancelAction
    {
        get => (CancelAction)List.Find(a => a is CancelAction) ?? _cancelAction;
        set
        {
            _cancelAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("formEditAction")]
    public FormEditAction FormEditAction
    {
        get => (FormEditAction)List.Find(a => a is FormEditAction) ?? _formEditAction;
        set
        {
            _formEditAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("auditLogFormToolbarAction")]
    public AuditLogFormToolbarAction AuditLogFormToolbarAction
    {
        get => (AuditLogFormToolbarAction)List.Find(a => a is AuditLogFormToolbarAction) ?? _auditLogFormToolbarAction;
        set
        {
            _auditLogFormToolbarAction = value;
            Set(value);
        }
    }

    
    public FormToolbarActionList() 
    {
        
    }
    
    private FormToolbarActionList(List<BasicAction> actions) : base(actions)
    {
     
    }
    
    public FormToolbarActionList DeepCopy()
    {
        return new FormToolbarActionList(List.ConvertAll(a=>a.DeepCopy()));
    }
}
