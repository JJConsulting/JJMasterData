using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Components;

public class ModalDialogFactory : IComponentFactory<JJModalDialog>
{
    public JJModalDialog Create()
    {
        return new JJModalDialog();
    }
}