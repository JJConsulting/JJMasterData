using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class FormToolbarActionList : FormElementActionList
{
    [JsonPropertyName("saveAction")]
    public SaveAction SaveAction
    {
        get => GetOrAdd<SaveAction>();
        set => Set(value);
    }

    [JsonPropertyName("backAction")]
    public BackAction BackAction
    {
        get => GetOrAdd<BackAction>();
        set => Set(value);
    }

    [JsonPropertyName("cancelAction")]
    public CancelAction CancelAction
    {
        get => GetOrAdd<CancelAction>();
        set => Set(value);
    }

    [JsonPropertyName("formEditAction")]
    public FormEditAction FormEditAction
    {
        get => GetOrAdd<FormEditAction>();
        set => Set(value);
    }

    [JsonPropertyName("auditLogFormToolbarAction")]
    public AuditLogFormToolbarAction AuditLogFormToolbarAction
    {
        get => GetOrAdd<AuditLogFormToolbarAction>();
        set => Set(value);
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
