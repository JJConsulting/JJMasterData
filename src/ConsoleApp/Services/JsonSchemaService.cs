using JJMasterData.ConsoleApp.Writers;

namespace JJMasterData.ConsoleApp.Services;

public class JsonSchemaService
{
    private JJMasterDataOptionsWriter MasterDataOptionsWriter { get; }
    private FormElementWriter FormElementWriter { get; }

    public JsonSchemaService(JJMasterDataOptionsWriter masterDataOptionsWriter, FormElementWriter formElementWriter)
    {
        MasterDataOptionsWriter = masterDataOptionsWriter;
        FormElementWriter = formElementWriter;
    }
    
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