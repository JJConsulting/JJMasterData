using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Test.Extensions;

public class DeepCopyTest
{
    [Fact]
    public void ObjectClonerDeepCopyTest()
    {
        var obj1 = new object();
        var obj2 = ObjectCloner.DeepCopy(obj1);
        
        Assert.NotEqual(obj1,obj2);
    }
}