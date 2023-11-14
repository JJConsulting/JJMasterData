using JJMasterData.ConsoleApp.Writers;

namespace JJMasterData.ConsoleApp.Services;

public class JsonSchemaService(JJMasterDataOptionsWriter masterDataOptionsWriter, FormElementWriter formElementWriter)
{
    private JJMasterDataOptionsWriter MasterDataOptionsWriter { get; } = masterDataOptionsWriter;
    private FormElementWriter FormElementWriter { get; } = formElementWriter;

    public void GenerateJsonSchema(string schemaName)
    {
        switch (schemaName)
        {
            case "JJMasterDataOptions":
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