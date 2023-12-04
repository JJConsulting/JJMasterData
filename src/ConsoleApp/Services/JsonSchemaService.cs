using JJMasterData.ConsoleApp.Writers;

namespace JJMasterData.ConsoleApp.Services;

public class JsonSchemaService(MasterDataOptionsWriter masterDataOptionsWriter, FormElementWriter formElementWriter)
{
    private MasterDataOptionsWriter MasterDataOptionsWriter { get; } = masterDataOptionsWriter;
    private FormElementWriter FormElementWriter { get; } = formElementWriter;

    public void GenerateJsonSchema(string schemaName)
    {
        switch (schemaName)
        {
            case "MasterDataOptions":
                MasterDataOptionsWriter.Write();
                break;
            case "FormElement":
                FormElementWriter.Write();
                break;
            default:
                Console.Write("Invalid schema name.");
                break;
        }
    }
}