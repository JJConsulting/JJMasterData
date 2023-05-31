using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using Newtonsoft.Json;

namespace JJMasterData.Core.Test.DataDictionary;

public class FormElementTest
{
    [Fact]
    public void TestSerialization()
    {
        var form = GetFormElement();
        var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };
        string json = JsonConvert.SerializeObject(form, settings);
        var formElement = JsonConvert.DeserializeObject<FormElement>(json, settings);
        
        Assert.Equal(formElement!.Name, form.Name);
        Assert.Equal(formElement.Title, form.Title);
        Assert.Equal(formElement.Fields[0].Name, form.Fields[0].Name);
        Assert.Equal(formElement.Fields[0].Label, form.Fields[0].Label);
        Assert.Equal(formElement.Fields[0].Component, form.Fields[0].Component);
    }

    private FormElement GetFormElement()
    {
        var formElement = new FormElement
        {
            Name = "Test",
            TableName = "tb_teste",
            Title = "Test Title"
        };

        var field1 = new FormElementField
        {
            Name = "id",
            Label = "Id",
            IsPk = true,
            AutoNum = true,
            DataType = FieldType.Int,
            Component = FormComponent.Number
        };
        formElement.Fields.Add(field1);

        var field2 = new FormElementField
        {
            Name = "description",
            Label = "Description",
            DataType = FieldType.Varchar,
            Size = 50
        };
        
        formElement.Fields.Add(field2);

        return formElement;
    }

}