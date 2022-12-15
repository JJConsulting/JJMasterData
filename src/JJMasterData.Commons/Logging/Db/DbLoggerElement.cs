using JJMasterData.Commons.Dao.Entity;

namespace JJMasterData.Commons.Logging.Db;

public static class DbLoggerElement
{
    private static Element _element;
    
    public static Element GetInstance(string tableName)
    {

        if (_element != null)
            return _element;
        
        _element = new Element
        {
            Name = "Logger",
            TableName = tableName,
            Fields =
            {
                new ElementField
                {
                    Name = "Created",
                    Label = "Created",
                    DataType = FieldType.DateTime,
                    Filter = new ElementFilter
                    {
                        Type = FilterMode.Range
                    },
                    IsPk = true
                },
                new ElementField
                {
                    Name = "LogLevel",
                    Label = "LogLevel",
                    DataType = FieldType.Int,
                    Filter = new ElementFilter
                    {
                        Type = FilterMode.Equal
                    }
                },
                new ElementField
                {
                    Name = "EventId",
                    Label = "EventId",
                    DataType = FieldType.Int
                },
                new ElementField
                {
                    Name = "EventName",
                    Label = "Event Name",
                    Size = 100,
                    DataType = FieldType.Varchar
                },
                new ElementField
                {
                    Name = "Message",
                    Label = "Message",
                    Size = 700,
                    DataType = FieldType.Varchar
                },   
                new ElementField
                {
                    Name = "ExceptionMessage",
                    Label = "Exception Message",
                    Size = 700,
                    DataType = FieldType.Varchar
                },               
                new ElementField
                {
                    Name = "ExceptionStackTrace",
                    Label = "Exception StackTrace",
                    Size = 700,
                    DataType = FieldType.Varchar
                },
                new ElementField
                {
                    Name = "ExceptionSource",
                    Label = "Exception Source",
                    Size = 700,
                    DataType = FieldType.Varchar
                }
            }
        };

        return _element;
    }
    
}