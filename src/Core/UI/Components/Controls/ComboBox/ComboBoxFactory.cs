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

internal class ComboBoxFactory : IControlFactory<JJComboBox>
{
    private IHttpContext HttpContext { get; }
    private IEntityRepository EntityRepository { get; }
    private IExpressionsService ExpressionsService { get; }
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private ILoggerFactory LoggerFactory { get; }

    public ComboBoxFactory(IHttpContext httpContext, IEntityRepository entityRepository,
        IExpressionsService expressionsService, IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    {
        HttpContext = httpContext;
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        LoggerFactory = loggerFactory;
    }

    public JJComboBox Create()
    {
        return new JJComboBox(HttpContext, EntityRepository, ExpressionsService,
            StringLocalizer,
            LoggerFactory.CreateLogger<JJComboBox>());
    }

    public JJComboBox Create(FormElement formElement, FormElementField field, ControlContext controlContext)
    {
        var formStateData = controlContext.FormStateData;

        var comboBox = Create();
        comboBox.DataItem = field.DataItem;
        comboBox.Name = field.Name;
        comboBox.Visible = true;
        comboBox.FormValues = formStateData.FormValues;
        comboBox.MultiSelect = field.DataItem!.EnableMultiSelect;
        comboBox.PageState = formStateData.PageState;
        comboBox.SelectedValue = controlContext.Value?.ToString();
        comboBox.UserValues = formStateData.UserValues;

        return comboBox;
    }
}