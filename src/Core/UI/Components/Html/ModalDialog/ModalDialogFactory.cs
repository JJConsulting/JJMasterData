namespace JJMasterData.Core.UI.Components;

public class ModalDialogFactory : IComponentFactory<JJModalDialog>
{
    public JJModalDialog Create()
    {
        return new JJModalDialog();
    }
}