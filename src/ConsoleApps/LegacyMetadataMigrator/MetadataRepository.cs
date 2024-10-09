#nullable  disable

using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.LegacyMetadataMigrator.FormElementMigration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace JJMasterData.LegacyMetadataMigrator;

public class MetadataRepository(IEntityRepository entityRepository, IOptions<MasterDataCoreOptions> options)
{
    private Element _masterDataElement;

    internal Element MasterDataElement
    {
        get
        {
            if (_masterDataElement == null)
            {
                var tableName = options.Value.DataDictionaryTableName;
                _masterDataElement = DataDictionaryStructure.GetElement(tableName);
            }
            return _masterDataElement;
        }
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetFormElementListAsync"/>
    public IEnumerable<Metadata> GetMetadataList(bool? sync = null)
    {
        var list = new List<Metadata>();
        var entityParameters = new EntityParameters();
        entityParameters.OrderBy.AddOrReplace("name", OrderByDirection.Asc);
        entityParameters.OrderBy.AddOrReplace("type", OrderByDirection.Asc);
        if (sync.HasValue)
            entityParameters.Filters.Add("sync", (bool)sync ? "1" : "0");
        
        MasterDataElement.Fields.Add(new ElementField
        {
            Name = "namefilter",
            Filter = new ElementFilter(FilterMode.Contain),
            DataBehavior = FieldBehavior.ViewOnly
        });

        //Ignore procedures to apply compatibility
        MasterDataElement.UseReadProcedure = false;
        MasterDataElement.UseWriteProcedure = false;

        string currentName = "";
        var dt = entityRepository.GetDictionaryListResultAsync(MasterDataElement,entityParameters, false).GetAwaiter().GetResult();
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
            var type = row["type"].ToString()!;
            switch (type)
            {
                case "T":
                    currentParser!.Table = JsonConvert.DeserializeObject<Element>(json);
                    break;
                case "F":
                    currentParser!.Form = JsonConvert.DeserializeObject<MetadataForm>(json, new JsonSerializerSettings
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
            if (dicParser.Table.EnableSynchronism)
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
        if (dicParser.Form is { Panels: null }) dicParser.Form.Panels = [];

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
                    field.DataItem.DataItemType = DataItemType.ElementMap;
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