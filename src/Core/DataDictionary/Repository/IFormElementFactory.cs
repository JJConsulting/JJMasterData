namespace JJMasterData.Core.DataDictionary.Repository;

public interface IFormElementFactory
{
    string ElementName { get; }
    FormElement GetFormElement();
}