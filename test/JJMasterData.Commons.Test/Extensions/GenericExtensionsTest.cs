using JJMasterData.Commons.Dao.Entity;
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

    [Fact]
    public void ToDictionaryTest()
    {
        const string formElementName = "FormElementTest";
        const string formElementFieldName = "FieldTest";
        
        var formElement = new Element
        {
            Name = formElementName
        };
        formElement.Fields.Add(new()
        {
            Name = formElementFieldName
        });

        var dictionary = formElement.ToDictionary();
        
        Assert.Equal(formElementName, dictionary["name"]);
        Assert.Equal(formElementFieldName, dictionary["fields[0].fieldname"]);
    }
}