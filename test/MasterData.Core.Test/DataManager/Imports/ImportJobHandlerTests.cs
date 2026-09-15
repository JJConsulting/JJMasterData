using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Importation;
using JJMasterData.Core.DataManager.Importation.Background;

namespace JJMasterData.Core.Test.DataManager.Imports;

public class ImportJobHandlerTests
{
    [Fact]
    public void FillsOmittedTrailingOptionalField()
    {
        List<FormElementField> fields =
        [
            new() { Name = "Required", IsRequired = true },
            new() { Name = "Optional" }
        ];
        var record = new ImportRecord(1, ["value"]);

        var isValid = ImportJobHandler.TryFillMissingOptionalFields(fields, record);

        Assert.True(isValid);
        Assert.Equal(2, record.Values.Count);
        Assert.Null(record.Values[1]);
    }

    [Fact]
    public void RejectsOmittedTrailingRequiredField()
    {
        List<FormElementField> fields =
        [
            new() { Name = "First" },
            new() { Name = "Required", IsRequired = true }
        ];
        var record = new ImportRecord(1, ["value"]);

        var isValid = ImportJobHandler.TryFillMissingOptionalFields(fields, record);

        Assert.False(isValid);
        Assert.Single(record.Values);
    }

    [Fact]
    public void RejectsExtraFields()
    {
        List<FormElementField> fields = [new() { Name = "Only" }];
        var record = new ImportRecord(1, ["value", "extra"]);

        var isValid = ImportJobHandler.TryFillMissingOptionalFields(fields, record);

        Assert.False(isValid);
        Assert.Equal(2, record.Values.Count);
    }
}
