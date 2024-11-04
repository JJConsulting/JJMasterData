#nullable enable
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Localization;

public sealed class MasterDataStringLocalizer<TResourceSource>(IStringLocalizerFactory factory)
	: IStringLocalizer<TResourceSource>
{
	private readonly IStringLocalizer _localizer = factory.Create(typeof(TResourceSource));

	/// <inheritdoc />
	public LocalizedString this[string? name] => _localizer[name!];

	/// <inheritdoc />
	public LocalizedString this[string? name, params object[] arguments] => _localizer[name!, arguments];

	/// <inheritdoc />
	public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
		_localizer.GetAllStrings(includeParentCultures);
}

public sealed class MasterDataStringLocalizer(
	string resourceName,
	IStringLocalizer resourcesStringLocalizer,
	IEntityRepository entityRepository,
	IMemoryCache cache,
	IOptionsMonitor<MasterDataCommonsOptions> options)
	: IStringLocalizer
{
	private static readonly LocalizedString Empty = new(string.Empty, string.Empty, resourceNotFound:true);
	
	public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
	{
		return GetAllStringsAsDictionary().Select(e => new LocalizedString(e.Key, e.Value));
	}

	public LocalizedString this[string? name]
	{
		get
		{
			if (string.IsNullOrEmpty(name))
				return Empty;

			var value = GetString(name!);
			return new LocalizedString(name!, value, false);
		}
	}

	public LocalizedString this[string? name, params object[] arguments]
	{
		get
		{
			if (string.IsNullOrEmpty(name))
				return Empty;

			return new LocalizedString(name!, string.Format(this[name], arguments));
		}
	}
	
	private string GetString(string key)
	{
		var culture = Thread.CurrentThread.CurrentCulture.Name;
		var cacheKey = $"{resourceName}_localization_strings_{culture}";

		if (cache.TryGetValue<FrozenDictionary<string, string>>(cacheKey, out var cachedDictionary))
		{
			return cachedDictionary.GetValueOrDefault(key, key);
		}

		var localizedStrings = GetAllStringsAsDictionary();

		cache.Set(cacheKey, localizedStrings);

		return localizedStrings.GetValueOrDefault(key, key);
	}

	private FrozenDictionary<string, string> GetAllStringsAsDictionary()
	{
		var culture = Thread.CurrentThread.CurrentCulture.Name;

		var element = MasterDataStringLocalizerElement.GetElement(options.CurrentValue);

		var hasConnectionString = !string.IsNullOrEmpty(options.CurrentValue.ConnectionString);

		var tableExists = hasConnectionString && entityRepository.TableExists(element.TableName);

		if (!tableExists && hasConnectionString)
			entityRepository.CreateDataModel(element, []);

		var stringLocalizerValues = GetStringLocalizerValues();
		var databaseValues =
			hasConnectionString ? GetDatabaseValues(element, culture) : new Dictionary<string, object?>();

		if (databaseValues.Count > 0)
		{
			foreach (var dbValue in databaseValues.ToList())
			{
				stringLocalizerValues[dbValue.Key] = dbValue.Value?.ToString() ?? string.Empty;
			}
		}
		
		return stringLocalizerValues.ToFrozenDictionary();
	}
	
	private Dictionary<string, string> GetStringLocalizerValues()
	{
		try
		{
			var values = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			var localizedStrings = resourcesStringLocalizer.GetAllStrings();

			foreach (var localizedString in localizedStrings)
			{
				if(!values.ContainsKey(localizedString.Name))
					values.Add(localizedString.Name, localizedString.Value);
			}

			return values;
		}
		catch
		{
			return new Dictionary<string, string>();
		}
	}

	private Dictionary<string, object?> GetDatabaseValues(Element element, string culture)
	{
		var values = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
		var filter = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "cultureCode", culture }
		};
		var result =
			entityRepository.GetDictionaryListResult(element, new EntityParameters { Filters = filter }, false);
		foreach (var row in result.Data)
		{
			values.Add(row["resourceKey"]!.ToString()!, row["resourceValue"]?.ToString());
		}

		return values;
	}
}