#nullable enable
namespace JJMasterData.Core.UI.Components;

public interface IFloatingLabelControl
{
    public string? FloatingLabel { get; set; }
    public bool UseFloatingLabel { get; set; }
}