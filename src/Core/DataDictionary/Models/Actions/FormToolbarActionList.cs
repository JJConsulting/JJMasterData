using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class FormToolbarActionList : FormElementActionList
{
    public SaveAction SaveAction => List.First(a => a is SaveAction) as SaveAction;
    
    public BackAction BackAction => List.First(a => a is BackAction) as BackAction;
    
    public CancelAction CancelAction => List.First(a => a is CancelAction) as CancelAction;
    
    public FormEditAction FormEditAction => List.First(a => a is FormEditAction) as FormEditAction;
    public AuditLogFormToolbarAction AuditLogFormToolbarAction => List.First(a => a is AuditLogFormToolbarAction) as AuditLogFormToolbarAction;
    public FormToolbarActionList() 
    {
        List.Add(new SaveAction());
        List.Add(new CancelAction());
        List.Add(new BackAction());
        List.Add(new FormEditAction());
    }
    
    [JsonConstructor]
    private FormToolbarActionList(List<BasicAction> list)
    {
        List = list;

        EnsureActionExists<CancelAction>();
        EnsureActionExists<BackAction>();
        EnsureActionExists<FormEditAction>();
        EnsureActionExists<AuditLogFormToolbarAction>();
    }

    public FormToolbarActionList DeepCopy()
    {
        return new FormToolbarActionList(List.Select(a=>a.DeepCopy()).ToList());
    }
}