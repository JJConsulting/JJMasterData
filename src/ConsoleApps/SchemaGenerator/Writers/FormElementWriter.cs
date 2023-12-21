using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.SchemaGenerator.Writers;

public class FormElementWriter : BaseWriter
{
    public virtual void Write()
    {
        var schema = Generator.Generate(typeof(FormElement));
        WriteSchema("FormElement", schema);
    }
}