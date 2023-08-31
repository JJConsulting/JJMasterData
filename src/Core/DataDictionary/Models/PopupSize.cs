using System.ComponentModel;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Tamanho da janela do popup
/// </summary>
public enum ModalSize
{
    [Description("modal-fullscreen")]
    Fullscreen,

    [Description("jj-modal-xl")]
    ExtraLarge,

    [Description("jj-modal-lg")]
    Large,

    [Description("jj-modal-default")]
    Default,

    [Description("jj-modal-sm")]
    Small
}