using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Extensions;

namespace JJMasterData.Commons.Test.Extensions;

public class GenericExtensionsTest
{
    [Fact]
    public void DeepCopyTest()
    {
        var obj1 = new object();
        var obj2 = obj1.DeepCopy();
        
        Assert.NotEqual(obj1,obj2);
    }
}