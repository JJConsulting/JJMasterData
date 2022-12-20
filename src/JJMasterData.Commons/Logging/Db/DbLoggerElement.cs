using JJMasterData.Commons.Dao.Entity;

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
            Name = "Logging",
            TableName = options.TableName,
            Info = "System Log",
            Fields =
            {
                new ElementField
                {
                    Name = options.CreatedColumnName,
                    Label = "Created",
                    IsPk = true,
                    DataType = FieldType.DateTime,
                    Filter =
                    {
                        Type = FilterMode.Range
                    }
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
                    Name = options.EventColumnName,
                    Label = "Event",
                    DataType = FieldType.Varchar,
                    Size = 50,
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
                    Size = 8000,
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