using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Commons.Logging.Db;

public static class DbLoggerElement
{
    private static Element _element;
    
    public static Element GetInstance(DbLoggerOptions options)
    {
        if (_element != null)
            return _element;

        _element = new Element
        {
            Name = options.TableName,
            Schema = options.TableSchema,
            TableName = options.TableName,
            UseReadProcedure = false,
            UseWriteProcedure = false,
            Info = "System Log",
            Fields =
            {
                new ElementField
                {
                    Name = options.IdColumnName,
                    Label = "Id",
                    IsPk = true,
                    AutoNum = true,
                    DataType = FieldType.Int,
                    Size = 5
                },
                new ElementField
                {
                    Name = options.LevelColumnName,
                    Label = "Level",
                    DataType = FieldType.Int,
                    Size = 1,
                    Filter =
                    {
                        Type = FilterMode.Equal
                    }
                },
                new ElementField
                {
                    Name = options.CreatedColumnName,
                    Label = "Created",
                    DataType = FieldType.DateTime2,
                    Size = 5,
                    Filter =
                    {
                        Type = FilterMode.Range
                    }
                },
                new ElementField
                {
                    Name = options.CategoryColumnName,
                    Label = "Category",
                    DataType = FieldType.Varchar,
                    Size = -1,
                    Filter =
                    {
                        Type = FilterMode.Contain
                    }
                },
                new ElementField
                {
                    Name = options.MessageColumnName,
                    Label = "Message",
                    DataType = FieldType.Varchar,
                    Size = -1,
                    Filter =
                    {
                        Type = FilterMode.Contain
                    }
                }
            }
        };
        
        return _element;
        
    }
    
}