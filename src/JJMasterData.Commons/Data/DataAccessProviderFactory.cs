using System;
using System.ComponentModel;
using System.Data.Common;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Util;
using Microsoft.Data.SqlClient;

namespace JJMasterData.Commons.Data;

public static class DataAccessProviderFactory
{
    public static DataAccessProvider GetDataAccessProviderFromString(string description)
    {
        foreach(var field in typeof(DataAccessProvider).GetFields())
        {
            if (Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (attribute.Description == description)
                    return (DataAccessProvider)field.GetValue(null);
            }
            else
            {
                if (field.Name == description)
                    return (DataAccessProvider)field.GetValue(null);
            }
        }
        
        return default;
    }
    public static DbProviderFactory GetDbProviderFactory(string dbProviderFactoryTypename, string assemblyName)
    {
        var instance = ReflectionUtils.GetStaticProperty(dbProviderFactoryTypename, "Instance");
        if (instance == null)
        {
            var assembly = ReflectionUtils.LoadAssembly(assemblyName);
            if (assembly != null)
                instance = ReflectionUtils.GetStaticProperty(dbProviderFactoryTypename, "Instance");
        }

        if (instance == null)
            throw new DataAccessProviderException($"Error loading {dbProviderFactoryTypename} from {assemblyName}");
        
        
        return instance as DbProviderFactory;
    }

    public static DbProviderFactory GetDbProviderFactory(DataAccessProvider type)
    {
        switch (type)
        {
            case DataAccessProvider.SqlServer:
                return SqlClientFactory.Instance; // this library has a ref to SqlClient so this works
            case DataAccessProvider.Oracle:
                return GetDbProviderFactory(type.GetDescription(), "Oracle.ManagedDataAccess");
            case DataAccessProvider.OracleNetCore:
                return GetDbProviderFactory(type.GetDescription(), "Oracle.ManagedDataAccess.Core");
            case DataAccessProvider.SqLite:
            case DataAccessProvider.MySql:
                return GetDbProviderFactory(type.GetDescription(), "MySql.Data");
            case DataAccessProvider.PostgreSql:
                return GetDbProviderFactory(type.GetDescription(), "Npgsql");
            default:
                throw new NotSupportedException($"Not supported {type}");
        }
    }
    
    public static DbProviderFactory GetDbProviderFactory(string providerName)
    {
        var type = GetDataAccessProviderFromString(providerName);
        return GetDbProviderFactory(type);
    }

}