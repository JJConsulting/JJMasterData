using System;
using JJMasterData.Commons.Security;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class HierarchyFactory(
    IHttpContextAccessor httpContextAccessor,
    DataProtectionService dataProtectionService,
    DataItemService dataItemService,
    IStringLocalizer<MasterDataResources> stringLocalizer)
    : IControlFactory<JJHierarchy>
{
    public JJHierarchy Create() =>
        new(httpContextAccessor, dataProtectionService, dataItemService, stringLocalizer);

    public JJHierarchy Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        if (field.DataItem is null)
            throw new ArgumentNullException(nameof(field.DataItem));

        return new JJHierarchy(httpContextAccessor, dataProtectionService, dataItemService, stringLocalizer)
        {
            ConnectionId = formElement.ConnectionId,
            DataItem = field.DataItem,
            Name = field.Name,
            FieldName = field.Name,
            ElementName = formElement.Name,
            ParentElementName = formElement.ParentName,
            Visible = true,
            FormStateData = context.FormStateData,
            SelectedValue = context.Value?.ToString(),
            UserValues = context.FormStateData.UserValues!,
            IsRequired = field.IsRequired,
            ShowAsModal = field.DataItem.ShowAsModal,
            ModalTitle = string.IsNullOrEmpty(field.Label) ? field.Name : stringLocalizer[field.Label],
            ShowSelectedPathOnly = context.FormStateData.PageState is PageState.View
        };
    }
}
