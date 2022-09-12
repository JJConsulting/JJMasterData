using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;

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
        const string formElementTitle = "FormElementTest";
        const string formElementFieldName = "FieldTest";
        
        var formElement = new FormElement
        {
            Title = formElementTitle
        };
        formElement.Fields.Add(new()
        {
            Name = formElementFieldName
        });

        var dictionary = formElement.ToDictionary();
        
        Assert.Equal(formElementTitle, dictionary["title"]);
        Assert.Equal(formElementFieldName, dictionary["fields[0].fieldname"]);
    }
}