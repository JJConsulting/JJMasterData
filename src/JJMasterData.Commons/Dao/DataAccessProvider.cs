using System;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Dao;

public static class DataAccessProvider
{
    public static DataAccessProviderType GetDataAccessProviderTypeFromString(string description)
    {
        foreach(var field in typeof(DataAccessProviderType).GetFields())
        {
            if (Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (attribute.Description == description)
                    return (DataAccessProviderType)field.GetValue(null);
            }
            else
            {
                if (field.Name == description)
                    return (DataAccessProviderType)field.GetValue(null);
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

    public static DbProviderFactory GetDbProviderFactory(DataAccessProviderType type)
    {
        switch (type)
        {
            case DataAccessProviderType.SqlServer:
                return SqlClientFactory.Instance; // this library has a ref to SqlClient so this works
            case DataAccessProviderType.Oracle:
                return GetDbProviderFactory(type.GetDescription(), "Oracle.ManagedDataAccess");
            case DataAccessProviderType.OracleNetCore:
                return GetDbProviderFactory(type.GetDescription(), "Oracle.ManagedDataAccess.Core");
            case DataAccessProviderType.SqLite:
            case DataAccessProviderType.MySql:
                return GetDbProviderFactory(type.GetDescription(), "MySql.Data");
            case DataAccessProviderType.PostgreSql:
                return GetDbProviderFactory(type.GetDescription(), "Npgsql");
            default:
                throw new NotSupportedException($"Not supported {type}");
        }
    }
    
    public static DbProviderFactory GetDbProviderFactory(string providerName)
    {
        var type = GetDataAccessProviderTypeFromString(providerName);
        return GetDbProviderFactory(type);
    }
}