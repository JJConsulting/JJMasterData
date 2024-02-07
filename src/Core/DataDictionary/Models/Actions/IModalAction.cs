namespace JJMasterData.Core.DataDictionary.Models.Actions;

public interface IModalAction
{
    public bool ShowAsModal { get; set; }    
    public string ModalTitle { get; set; }    
}