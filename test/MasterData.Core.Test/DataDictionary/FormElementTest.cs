using System.Data;
using System.Text.Json;
using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Models;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;



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
                    CssClass = "foo",
                    Export = true,
                    Filter = new()
                    {
                        Type = FilterMode.Contain,
                        IsRequired = true
                    },
                    NumberOfDecimalPlaces = 2,
                    EnableOnDelete = true,
                    VisibleExpression = "val:1",
                    PanelId = 1,
                    Attributes =
                    {
                        {
                            "example", "example"
                        }
                    },
                    IsRequired = true,
                    AutoPostBack = true,
                    DataBehavior = FieldBehavior.Real,
                    IsPk = true,
                    ValidateRequest = true,
                    DataItem = new FormElementDataItem
                    {
                        Command = new DataAccessCommand
                        {
                            Sql = "select",
                            Type = CommandType.Text,
                            Parameters =
                            {
                                new DataAccessParameter
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
                                Icon = FontAwesomeIcon.Adjust,
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
                        new ExportAction
                        {
                            ProcessOptions =
                            {
                                Scope = ProcessScope.User,
                                CommandAfterProcess = "select",
                            }
                        },
                        new ImportAction
                        {
                            ProcessOptions =
                            {
                                Scope = ProcessScope.Global
                            }
                        },
                        new SqlCommandAction
                        {
                            SqlCommand = "select",

                        }
                    ],
                    Component = FormComponent.Search
                },
                new FormElementField
                {
                    Name = "new field",
                    Label = "la",
                    Size = 1,
                    DataFile = new()
                    {
                        ShowAsUploadView = true
                    },
                    Actions =
                    [
                        new UrlRedirectAction
                        {
                            Name = "url",
                        },
                        new SqlCommandAction
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
                    },
                    ExportAction =
                    {
                        Name = "Test"
                    },
                    ConfigAction =
                    {
                        Name = "Test 2"
                    }
                },
                GridTableActions =
                {
                    ViewAction =
                    {
                        Text = "a",
                        Tooltip = "b",
                        IsDefaultOption = true,
                        IsGroup = true,
                        DividerLine = true,
                        Icon = FontAwesomeIcon.CircleONotch,
                        ShowTitle = true,
                        ConfirmationMessage = "a",
                        EnableExpression = "val:1",
                        VisibleExpression = "val:1",
                        Order = 1,
                        ShowAsButton = true,
                        CssClass = "css"
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
                new FormElementRelationship
                {
                    Panel = new()
                    {
                        Title = "panel"
                    },
                    ElementRelationship = new()
                    {
                        Columns =
                        {
                            new ElementRelationshipColumn
                            {
                                FkColumn = "A",
                                PkColumn = "B"
                            }
                        },
                        ChildElement = "ChildElement"
                    }
                }
            },
            ApiOptions = new FormElementApiOptions
            {
                EnableAdd = true
            },
            Panels =
            [
                new FormElementPanel
                {
                    Title = "Test",
                    Layout = PanelLayout.Panel
                }
            ],
            TableName = "tableName"
        };

        var newReference = formElement.DeepCopy();

        Assert.True(formElement != newReference);

        var oldJson = JsonSerializer.Serialize(formElement);

        var newJson = JsonSerializer.Serialize(formElement);

        Assert.Equal(oldJson, newJson);
    }
}