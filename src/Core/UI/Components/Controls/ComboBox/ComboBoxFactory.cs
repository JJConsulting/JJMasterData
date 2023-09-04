using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Factories;

internal class ComboBoxFactory : IControlFactory<JJComboBox>
{
    private IHttpRequest HttpRequest { get; }
    private IDataItemService DataItemService { get; }
    private IExpressionsService ExpressionsService { get; }
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private IEncryptionService EncryptionService { get; }
    private ILoggerFactory LoggerFactory { get; }

    public ComboBoxFactory(
        IHttpRequest httpRequest, 
        IDataItemService dataItemService,
        IExpressionsService expressionsService, 
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IEncryptionService encryptionService,
        ILoggerFactory loggerFactory)
    {
        HttpRequest = httpRequest;
        DataItemService = dataItemService;
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        EncryptionService = encryptionService;
        LoggerFactory = loggerFactory;
    }

    public JJComboBox Create()
    {
        return new JJComboBox(
            HttpRequest,
            DataItemService,
            ExpressionsService,
            StringLocalizer,
            EncryptionService,
            LoggerFactory.CreateLogger<JJComboBox>());
    }

    public JJComboBox Create(FormElement formElement, FormElementField field, ControlContext controlContext)
    {
        if (field.DataItem == null)
            throw new ArgumentNullException(nameof(field.DataItem));

        var comboBox = Create();
        comboBox.DataItem = field.DataItem;
        comboBox.Name = field.Name;
        comboBox.Visible = true;
        comboBox.FormStateData = controlContext.FormStateData;
        comboBox.MultiSelect = field.DataItem!.EnableMultiSelect;
        comboBox.SelectedValue = controlContext.Value?.ToString();
        comboBox.UserValues = controlContext.FormStateData.UserValues;

        return comboBox;
    }
}