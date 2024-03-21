using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Test.Util;

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