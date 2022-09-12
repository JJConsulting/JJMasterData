using System.Collections;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Extensions;

namespace JJMasterData.Commons.Test.Extensions;

public class HashtableExtensionsTest
{
    [Fact]
    public void ToModel_Test()
    {
        var hashtable = new Hashtable
        {
            { "Name", "Test" },
            { "Sync", true }
        };

        var model = hashtable.ToModel<Element>();

        Assert.NotNull(model);
        Assert.Equal("Test", model.Name);
        Assert.True(model.Sync);
     
    }
}