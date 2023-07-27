using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Factories;

public class ComboBoxFactory
{
    private IHttpContext HttpContext { get; }
    private IEntityRepository EntityRepository { get; }
    private IExpressionsService ExpressionsService { get; }
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private ILoggerFactory LoggerFactory { get; }

    public ComboBoxFactory(IHttpContext httpContext, IEntityRepository entityRepository,IExpressionsService expressionsService,IStringLocalizer<JJMasterDataResources> stringLocalizer, ILoggerFactory loggerFactory)
    {
        HttpContext = httpContext;
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        LoggerFactory = loggerFactory;
    }

    public JJComboBox CreateComboBox()
    {
        return new JJComboBox(HttpContext, EntityRepository, ExpressionsService,
            StringLocalizer,
            LoggerFactory.CreateLogger<JJComboBox>());
    }
    
    internal JJComboBox CreateComboBox(FormElementField f, ExpressionOptions expOptions, object value)
    {
        var comboBox = CreateComboBox();
        comboBox.DataItem = f.DataItem;
        comboBox.Name = f.Name;
        comboBox.Visible = true;
        comboBox.FormValues = expOptions.FormValues;
        comboBox.MultiSelect = f.DataItem!.EnableMultiSelect;
        comboBox.PageState = expOptions.PageState;
        comboBox.SelectedValue = value?.ToString();
        comboBox.UserValues = expOptions.UserValues;

        return comboBox;
    }
}