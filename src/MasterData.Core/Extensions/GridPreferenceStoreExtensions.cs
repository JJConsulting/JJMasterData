using System;
using System.Collections.Generic;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Extensions;

internal static class GridPreferenceStoreExtensions
{
    extension(IGridPreferenceStore store)
    {
        internal GridSettings? GetSettings(string gridName)
        {
            var settings = store.Get(gridName)?.Settings;
            return settings is { RecordsPerPage: >= 0, TotalPaginationButtons: > 0 } ? settings : null;
        }

        internal void SetSettings(string gridName, GridSettings? settings)
        {
            store.Update(gridName, preferences => preferences.Settings = settings);
        }

        internal Dictionary<string, object?>? GetFilter(string gridName)
        {
            return store.Get(gridName)?.Filter;
        }

        internal void SetFilter(string gridName, Dictionary<string, object?>? filter)
        {
            store.Update(gridName, preferences => preferences.Filter = filter is { Count: > 0 } ? filter : null);
        }

        internal string? GetOrder(string gridName)
        {
            return store.Get(gridName)?.Order;
        }

        internal void SetOrder(string gridName, string? order)
        {
            store.Update(gridName, preferences => preferences.Order = string.IsNullOrEmpty(order) ? null : order);
        }

        internal int? GetCurrentPage(string gridName)
        {
            var page = store.Get(gridName)?.CurrentPage;
            return page is > 0 ? page : null;
        }

        internal void SetCurrentPage(string gridName, int? page)
        {
            store.Update(gridName, preferences => preferences.CurrentPage = page is > 0 ? page : null);
        }

        private void Update(string gridName, Action<GridPreferences> update)
        {
            var preferences = store.Get(gridName) ?? new GridPreferences();
            update(preferences);
            store.Set(gridName, preferences is { Settings: null, Filter: null, Order: null, CurrentPage: null }
                ? null
                : preferences);
        }
    }
}
