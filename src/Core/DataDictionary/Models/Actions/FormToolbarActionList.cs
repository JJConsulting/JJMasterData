using System.Collections.Generic;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class FormToolbarActionList : FormElementActionList
{
    public SaveAction SaveAction { get; }
    public BackAction BackAction { get; }
    public CancelAction CancelAction { get; }
    public FormEditAction FormEditAction { get; }
    public AuditLogFormToolbarAction AuditLogFormToolbarAction { get; }

    public FormToolbarActionList()
    {
        SaveAction = new SaveAction();
        CancelAction = new CancelAction();
        BackAction = new BackAction();
        FormEditAction = new FormEditAction();
        AuditLogFormToolbarAction = new AuditLogFormToolbarAction();

        List.AddRange([
            SaveAction,
            CancelAction,
            BackAction,
            FormEditAction,
            AuditLogFormToolbarAction
        ]);
    }

    [JsonConstructor]
    private FormToolbarActionList(List<BasicAction> list)
    {
        List = list;

        SaveAction = EnsureActionExists<SaveAction>();
        CancelAction = EnsureActionExists<CancelAction>();
        BackAction = EnsureActionExists<BackAction>();
        FormEditAction = EnsureActionExists<FormEditAction>();
        AuditLogFormToolbarAction = EnsureActionExists<AuditLogFormToolbarAction>();
    }
    public FormToolbarActionList DeepCopy()
    {
        return new FormToolbarActionList(List.ConvertAll(a=>a.DeepCopy()));
    }
}