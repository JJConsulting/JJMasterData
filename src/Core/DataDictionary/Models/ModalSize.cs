namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Tamanho da janela do popup
/// </summary>
public enum ModalSize
{
    Fullscreen,
    ExtraLarge,
    Large,
    Default,
    Small
}

public static class ModalSizeExtensions
{
    public static string GetCssClass(this ModalSize modalSize)
    {
        return modalSize switch
        {
            ModalSize.Fullscreen => "modal-fullscreen",
            ModalSize.ExtraLarge => "jj-modal-xl",
            ModalSize.Large => "jj-modal-lg",
            ModalSize.Default => "jj-modal-default",
            ModalSize.Small => "jj-modal-sm",
            _ => "jj-modal-default"
        };
    }
}