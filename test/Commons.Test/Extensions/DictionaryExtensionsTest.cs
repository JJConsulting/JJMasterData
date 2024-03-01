using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Extensions;

namespace JJMasterData.Commons.Test.Extensions;

public class DictionaryExtensionsTest
{
    [Fact]
    public void ToModel_Test()
    {
        var dictionary = new Dictionary<string, object>
        {
            { "Name", "Test" },
            { "Sync", true }
        };

        var model = dictionary.ToModel<Element>();

        Assert.NotNull(model);
        Assert.Equal("Test", model.Name);
        Assert.True(model.EnableSynchronism);
     
    }
}