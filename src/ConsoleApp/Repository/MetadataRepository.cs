#nullable  disable

using System.Collections;
using System.Data;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Data.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.ConsoleApp.Models;
using JJMasterData.ConsoleApp.Models.FormElementMigration;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace JJMasterData.ConsoleApp.Repository;

public class MetadataRepository
{
    private readonly IEntityRepository _entityRepository;
    private readonly IOptions<JJMasterDataCoreOptions> _options;
    private Element _masterDataElement;

    internal Element MasterDataElement
    {
        get
        {
            if (_masterDataElement == null)
            {
                string tableName = _options.Value.DataDictionaryTableName;
                _masterDataElement = DataDictionaryStructure.GetElement(tableName);
            }
            return _masterDataElement;
        }
    }
 
    public MetadataRepository(IEntityRepository entityRepository, IOptions<JJMasterDataCoreOptions> options)
    {
        _entityRepository = entityRepository;
        _options = options;
    }
   
    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataList"/>
    public IEnumerable<Metadata> GetMetadataList(bool? sync = null)
    {
        var list = new List<Metadata>();

        var filter = new Hashtable();
        if (sync.HasValue)
            filter.Add("sync", (bool)sync ? "1" : "0");

        const string orderBy = "name, type";
        string currentName = "";
        int tot = 1;
        var dt = _entityRepository.GetDictionaryListAsync(MasterDataElement).GetAwaiter().GetResult();
        Metadata currentParser = null;
        foreach (var row in dt.Data)
        {
            string name = row["name"].ToString();
            if (!currentName.Equals(name))
            {
                ApplyCompatibility(currentParser);

                currentName = name;
                list.Add(new Metadata());
                currentParser = list[^1];
            }

            string json = row["json"].ToString();
            switch (row["type"].ToString()!)
            {
                case "T":
                    currentParser!.Table = JsonConvert.DeserializeObject<Element>(json);
                    break;
                case "F":
                    currentParser!.Form = JsonConvert.DeserializeObject<MetadataForm>(json, new JsonSerializerSettings()
                    {
                        Error = (sender, args) =>
                        {
                            args.ErrorContext.Handled = true;
                        }
                    });
                    break;
                case "L":
                    currentParser!.Options = JsonConvert.DeserializeObject<MetadataOptions>(json);
                    break;
                case "A":
                    currentParser!.ApiOptions = JsonConvert.DeserializeObject<MetadataApiOptions>(json);
                    break;
            }
        }

        ApplyCompatibility(currentParser);

        return list;
    }
    
        
    public static void ApplyCompatibility(Metadata dicParser)
    {
        if (dicParser?.Table == null)
            return;

        //Nairobi
        dicParser.Options ??= new MetadataOptions();

        dicParser.Options.ToolbarActions ??= new GridToolbarActions();

        dicParser.Options.GridActions ??= new GridActions();


        //Denver
        if (dicParser.ApiOptions == null)
        {
            dicParser.ApiOptions = new MetadataApiOptions();
            if (dicParser.Table.Sync)
            {
                dicParser.ApiOptions.EnableGetAll = true;
                dicParser.ApiOptions.EnableGetDetail = true;
                dicParser.ApiOptions.EnableAdd = true;
                dicParser.ApiOptions.EnableUpdate = true;
                dicParser.ApiOptions.EnableUpdatePart = true;
                dicParser.ApiOptions.EnableDel = true;
            }
        }

        if (string.IsNullOrEmpty(dicParser.Table.TableName))
        {
            dicParser.Table.TableName = dicParser.Table.Name;
        }

        //Tokio
        if (dicParser.Form is { Panels: null }) dicParser.Form.Panels = new List<FormElementPanel>();

        //Professor
        if (dicParser.Form != null)
        {
            foreach (var field in dicParser.Form.FormFields)
            {
                if (field.DataItem is not { DataItemType: DataItemType.Manual })
                    continue;

                if (field.DataItem.Command != null && !string.IsNullOrEmpty(field.DataItem.Command.Sql))
                    field.DataItem.DataItemType = DataItemType.SqlCommand;
                else if (field.DataItem.ElementMap != null && !string.IsNullOrEmpty(field.DataItem.ElementMap.ElementName))
                    field.DataItem.DataItemType = DataItemType.Dictionary;
            }
        }

        //Arturito
        foreach (var action in dicParser.Options.GridActions.GetAll()
                     .Where(action => action is UrlRedirectAction or InternalAction or ScriptAction or SqlCommandAction))
        {
            //action.IsUserCreated = true;
        }

        foreach (var action in dicParser.Options.ToolbarActions
                     .GetAll()
                     .Where(action => action is UrlRedirectAction or InternalAction or ScriptAction or SqlCommandAction))
        {
            //action.IsUserCreated = true;
        }
        

        //Sirius

        dicParser.Options.ToolbarActions.ExportAction.ProcessOptions ??= new ProcessOptions();

        dicParser.Options.ToolbarActions.ImportAction.ProcessOptions ??= new ProcessOptions();
    }
}