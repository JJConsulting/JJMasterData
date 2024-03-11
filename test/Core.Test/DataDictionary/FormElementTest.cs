using System.Data;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using Newtonsoft.Json;

namespace JJMasterData.Core.Test.DataDictionary;

public class FormElementTest
{
    [Fact]
    public void DeepCopyTest()
    {
        var formElement = new FormElement
        {
            Name = "test",
            Info = "test",
            Fields =
            {
                new FormElementField
                {
                    Name = "rt",
                    Label = "la",
                    Size = 1,
                    DataItem = new FormElementDataItem()
                    {
                        Command = new DataAccessCommand()
                        {
                            Sql = "select",
                            Type = CommandType.Text,
                            Parameters =
                            {
                                new DataAccessParameter()
                                {
                                    Name = "Test",
                                    Size = 1,
                                    Direction = ParameterDirection.Input
                                }
                            }
                        },
                        DataItemType = DataItemType.Manual,
                        Items =
                        [
                            new DataItemValue
                            {
                                Icon = IconType.Adjust,
                                Description = "Example",
                                Group = "Example",
                                Id = "Example",
                                IconColor = "#faaaa"
                            }
                        ],
                        ElementMap = new DataElementMap
                        {
                            Filters = new(),
                            ElementName = "element",
                            MapFilters =
                            [
                                new()
                                {
                                    FieldName = "b",
                                    ExpressionValue = "a"
                                }
                            ]
                        },
                        ShowIcon = true
                    },
                    Actions =
                    [
                        new ExportAction()
                        {
                            ProcessOptions =
                            {
                                Scope = ProcessScope.User,
                                CommandAfterProcess = "select",
                            }
                        },
                        new SqlCommandAction()
                        {
                            SqlCommand = "select",
                            
                        }
                    ],
                    Component = FormComponent.Search
                }
            },
            Options = new()
            {
                GridToolbarActions =
                {
                    ImportAction =
                    {
                        ProcessOptions =
                        {
                            CommandAfterProcess = "SELECT"
                        }
                    }
                },
                EnableAuditLog = true,
                Grid = new()
                {
                    EnableSorting = true,
                },
                Form = new()
                {
                    IsVerticalLayout = true
                }
            },
            Indexes =
            [
                new()
                {
                    IsUnique = true
                }
            ],
            Relationships =
            {
                new FormElementRelationship()
                {
                    Panel = new()
                    {
                        Title = "panel"
                    },
                    ElementRelationship = new()
                    {
                        Columns =
                        {
                            new ElementRelationshipColumn()
                            {
                                FkColumn = "A",
                                PkColumn = "B"
                            }
                        },
                        ChildElement = "ChildElement"
                    }
                }
            },
            ApiOptions = new FormElementApiOptions()
            {
                EnableAdd = true
            },
            Panels = [new FormElementPanel()
            {
                Title = "Test",
                Layout = PanelLayout.Panel
            }]
        };

        var newReference = formElement.DeepCopy();

        Assert.True(formElement != newReference);
        Action<JsonSerializerSettings>? configureSettings = options => { options.Formatting = Formatting.Indented; };
        var oldJson = FormElementSerializer.Serialize(formElement, configureSettings);

        var newJson = FormElementSerializer.Serialize(formElement, configureSettings);

        Assert.Equal(oldJson, newJson);
    }
}