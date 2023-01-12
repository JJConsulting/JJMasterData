using HtmlAgilityPack;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.WebComponents.Factories;

namespace JJMasterData.Core.Test.Web.Components;

public class JJGridViewTest
{
    public GridViewFactory GridViewFactory { get; }

    public JJGridViewTest(GridViewFactory gridViewFactory)
    {
        GridViewFactory = gridViewFactory;
    }

    private static FormElement GetFormElement()
    {
        return new FormElement
        {
            Name = "MyFormElement",
            Title = "My Example Form",
            Fields =
            {
                new FormElementField
                {
                    Name = "Field1"
                },
                new FormElementField
                {
                    Name = "Field2"
                }
            }
        };
    }
    
    [Fact]
    public void ToStringTest()
    {
        var formElement = GetFormElement();
        var gridView = GridViewFactory.CreateGridView(formElement);
        var html = gridView.GetHtmlBuilder();

        var document = new HtmlDocument();
        document.LoadHtml(html.ToString(true));
        
        Assert.Contains(document.DocumentNode.ChildNodes, node => node.OriginalName == "table");
    }
}