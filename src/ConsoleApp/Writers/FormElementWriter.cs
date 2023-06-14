using JJMasterData.Core.DataDictionary;

namespace JJMasterData.ConsoleApp.Writers;

public class FormElementWriter : BaseWriter
{
    public override void Write()
    {
        var schema = Generator.Generate(typeof(FormElement));
        WriteSchema("FormElement", schema);
    }
}