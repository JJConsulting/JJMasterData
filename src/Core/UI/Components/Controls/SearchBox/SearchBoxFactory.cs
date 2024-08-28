#nullable enable
using System;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal sealed class SearchBoxFactory(
        DataItemService dataItemService,
        IHttpRequest httpRequest,
        IEncryptionService encryptionService)
    : IControlFactory<JJSearchBox>
{
    public JJSearchBox Create()
    {
        return new JJSearchBox(httpRequest, encryptionService, dataItemService);
    }

    public JJSearchBox Create(FormElement formElement, FormElementField field, ControlContext controlContext)
    {
        if (field.DataItem == null)
            throw new ArgumentNullException(nameof(field.DataItem));

        var search = new JJSearchBox(httpRequest, encryptionService, dataItemService)
        {
            DataItem = field.DataItem,
            ConnectionId = formElement.ConnectionId,
            Name = field.Name,
            FieldName = field.Name,
            ParentElementName = formElement.ParentName,
            ElementName = formElement.Name,
            Visible = true,
            AutoReloadFormFields = false,
            FormStateData = controlContext.FormStateData,
            UserValues = controlContext.FormStateData.UserValues
        };

        if (controlContext.Value != null)
            search.SelectedValue = controlContext.Value.ToString()!;

        return search;
    }
    
}