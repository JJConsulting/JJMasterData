using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class FormToolbarActionList : FormElementActionList
{
    [JsonPropertyName("saveAction")]
    public SaveAction SaveAction
    {
        get => GetOrAdd<SaveAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("backAction")]
    public BackAction BackAction
    {
        get => GetOrAdd<BackAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("cancelAction")]
    public CancelAction CancelAction
    {
        get => GetOrAdd<CancelAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("formEditAction")]
    public FormEditAction FormEditAction
    {
        get => GetOrAdd<FormEditAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("auditLogFormToolbarAction")]
    public AuditLogFormToolbarAction AuditLogFormToolbarAction
    {
        get => GetOrAdd<AuditLogFormToolbarAction>();
        set => SetOfType(value);
    }
    
    public FormToolbarActionList()
    {
        Add(new SaveAction());
        Add(new BackAction());
        Add(new CancelAction());
        Add(new FormEditAction());
        Add(new AuditLogFormToolbarAction());
    }
    
    private FormToolbarActionList(List<BasicAction> actions) : base(actions)
    {
     
    }
    
    public FormToolbarActionList DeepCopy()
    {
        return new FormToolbarActionList(List.ConvertAll(a=>a.DeepCopy()));
    }
}
