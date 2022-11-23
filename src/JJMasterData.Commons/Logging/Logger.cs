using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging;

/// <summary>
/// JJMasterData ILogger implementation.
/// </summary>
public class Logger : ILogger
{
    private bool _checkTableLog;
    private Element _element;
    private IDataAccess _dataAccess;
    public IDataAccess DataAccess
    {
        get
        {
            if (_dataAccess != null) return _dataAccess;
            _dataAccess =
                Options.ConnectionStringName == null ? JJService.DataAccess : JJService.DataAccess.WithParameters(Options.ConnectionStringName);
            _dataAccess.GenerateLog = false;
            _dataAccess.TimeOut = 30;
            return _dataAccess;
        }
    }

    private Factory _factory;
    public Factory Factory => _factory ??= new Factory(DataAccess);

    public LoggerOptions Options { get; set; }

    private string CategoryName;

    public Logger()
    {
        Options = JJService.Options.Logger;
    }

    public Logger(string categoryName) : this()
    {
        CategoryName = categoryName;
    }

    /// <summary>
    /// Adicionar um log de erro, o sistema gravará o erro conforme as parametrizações da aplicação
    /// </summary>
    /// <param name="value">Descrição do erro</param>
    /// <param name="source">Origem ou Categoria</param>
    /// <remarks>Lucio Pelinson 2014-05-12</remarks>
    public void AddError(string value, string source = null)
    {
        Add(value, source, LoggerLevel.Error);
    }

    /// <summary>
    /// Adicionar um log de informação, o sistema gravará o log conforme as parametrizações da aplicação
    /// </summary>
    /// <param name="value">Descrição do log</param>
    /// <param name="source">Origem ou Categoria</param>
    /// <remarks>Lucio Pelinson 2014-05-12</remarks>
    public void AddInfo(string value, string source = null)
    {
        Add(value, source, LoggerLevel.Information);
    }

    /// <summary>
    /// Adicionar um log de aviso, o sistema gravará o log conforme as parametrizações da aplicação
    /// </summary>
    /// <param name="value">Descrição do aviso</param>
    /// <param name="source">Origem ou Categoria</param>
    /// <remarks>Lucio Pelinson 2014-05-12</remarks>
    public void AddWarning(string value, string source = null)
    {
        Add(value, source, LoggerLevel.Warning);
    }

    /// <summary>
    /// Adicionar um log, o sistema gravará o log conforme as parametrizações da aplicação
    /// </summary>
    /// <param name="value">Descrição do log</param>
    /// <param name="source">Origem ou Categoria</param>
    /// <param name="level">Tipo do log</param>
    /// <remarks>Lucio Pelinson 2014-05-12</remarks>
    public void Add(string value, string source, LoggerLevel level)
    {

        if (EnableOption(Options.WriteInTrace, level))
            WriteInTrace(value, source, level);

        if (EnableOption(Options.WriteInConsole, level))
            WriteInConsole(value, source, level);

        if (EnableOption(Options.WriteInFile, level))
            WriteInFile(value, source, level, Options.FileName);

        if (EnableOption(Options.WriteInEventViewer, level))
            WriteInEventViewer(value, source, level);

        if (EnableOption(Options.WriteInDatabase, level))
            WriteInDatabase(value, source, level);

    }

    private bool EnableOption(LoggerOption option, LoggerLevel level)
    {
        return option == LoggerOption.All || option.Equals(level);
    }

    public void WriteInTrace(string value, string source, LoggerLevel type)
    {
        if (source != null)
            value = "[" + source + "] " + value;


        if (type == LoggerLevel.Error)
            Trace.TraceError(value);
        else if (type == LoggerLevel.Warning)
            Trace.TraceWarning(value);
        else
            Trace.TraceInformation(value);
    }

    public void WriteInFile(string value, string source, LoggerLevel type, string fullnamearq)
    {
        if (fullnamearq == null)
            throw new ArgumentException(nameof(fullnamearq));

        var builder = new StringBuilder();
        builder.Append(DateTime.Now);
        builder.Append(" ");
        if (source != null)
        {
            builder.Append("[");
            builder.Append(source);
            builder.Append("] ");
        }
        builder.Append("(");
        builder.Append(type.ToString());
        builder.AppendLine(")");
        builder.Append(value);
        builder.AppendLine(" ");

        int erroCounter = 0;
        bool isReady = false;
        while (!isReady)
        {
            try
            {
                fullnamearq = FileIO.ResolveFilePath(fullnamearq);
                StreamWriter stream = new StreamWriter(fullnamearq, true);
                stream.WriteLine(builder.ToString());
                stream.Flush();
                stream.Close();
                isReady = true;
            }
            catch (DirectoryNotFoundException)
            {
                string folderPath = Path.GetDirectoryName(fullnamearq);
                if (folderPath != null)
                    Directory.CreateDirectory(folderPath);

                if (++erroCounter == 5)
                    isReady = true;
            }
            catch
            {
                Thread.Sleep(500);
                if (++erroCounter == 5)
                    isReady = true;
            }
        }
    }

    public void WriteInConsole(string value, string source, LoggerLevel type)
    {
        if (type == LoggerLevel.Error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("*".PadRight(37, '*'));
            Console.Write($" {Translate.Key("Error")} ");
            Console.WriteLine("*".PadRight(37, '*'));
            Console.ResetColor();
        }

        StringBuilder str = new StringBuilder();
        str.Append(DateTime.Now);
        str.Append(" ");
        if (source != null)
        {
            str.Append("[");
            str.Append(source);
            str.Append("] ");
        }
        str.Append(value);

        Console.WriteLine(str.ToString());

        if (type == LoggerLevel.Error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("*".PadRight(80, '*'));
            Console.ResetColor();
        }
    }

    public void WriteInEventViewer(string value, string source, LoggerLevel type)
    {
        EventLog log = new EventLog();
        log.Log = "JJConsulting";
        log.Source = source ?? "SYSTEM";
        if (type == LoggerLevel.Error)
        {
            log.WriteEntry(value, EventLogEntryType.Error);
        }
        else if (type == LoggerLevel.Warning)
        {
            log.WriteEntry(value, EventLogEntryType.Warning);
        }
        else
        {
            log.WriteEntry(value, EventLogEntryType.Information);
        }
        log.Close();
    }

    public void WriteInDatabase(string value, string source, LoggerLevel type)
    {
        try
        {
            if (string.IsNullOrEmpty(Options.ConnectionStringName))
                return;

            var element = GetElement();
            if (!LogTableExists())
                Factory.CreateDataModel(element);

            if (value.Length > 699)
            {
                value = value.Substring(0, 699);
                WriteInConsole("Arquivo de log truncado, maior que 700 chars", source, LoggerLevel.Warning);
            }

            if (source == null)
                source = "SYSTEM";
            else if (source.Length > 49)
                source = source.Substring(0, 49);

            Factory.Insert(element, new Hashtable
            {
                { Options.Table.DateColumnName, DateTime.Now },
                { Options.Table.LevelColumnName, type.ToString().Substring(0, 1) },
                { Options.Table.SourceColumnName, source },
                { Options.Table.ContentColumnName, value }
            });
        }
        catch (Exception ex)
        {
            var error = new StringBuilder();
            error.AppendLine(Translate.Key("Error while logging to the database."));
            error.AppendLine(ex.Message);
            WriteInConsole(error.ToString(), source, LoggerLevel.Error);
        }
    }

    public Element GetElement()
    {
        if (_element != null)
            return _element;

        _element = new Element
        {
            Name = "Logging",
            TableName = Options.Table.Name,
            Info = Translate.Key("System Log"),
        };

        var eventDate = new ElementField
        {
            Name = Options.Table.DateColumnName,
            Label = Translate.Key("Date"),
            IsPk = true,
            DataType = FieldType.DateTime
        };
        eventDate.Filter.Type = FilterMode.Range;
        _element.Fields.Add(eventDate);

        var type = new ElementField
        {
            Name = Options.Table.LevelColumnName,
            Label = Translate.Key("Type"),
            DataType = FieldType.Varchar,
            Size = 1
        };
        type.Filter.Type = FilterMode.Equal;
        _element.Fields.Add(type);

        var source = new ElementField
        {
            Name = Options.Table.SourceColumnName,
            Label = Translate.Key("Source"),
            DataType = FieldType.Varchar,
            Size = 50
        };
        source.Filter.Type = FilterMode.Contain;
        _element.Fields.Add(source);

        var msg = new ElementField
        {
            Name = Options.Table.ContentColumnName,
            Label = Translate.Key("Message"),
            DataType = FieldType.Varchar,
            Size = 700
        };
        msg.Filter.Type = FilterMode.Contain;
        _element.Fields.Add(msg);

        return _element;
    }
    

    public void ClearLog()
    {
        string sql = string.Format("TRUNCATE TABLE {0}", Options.Table.Name);
        var dataAccess = JJService.DataAccess;
        dataAccess.SetCommand(sql);
    }

    public bool LogTableExists()
    {
        if (_checkTableLog)
            return true;

        if (!JJService.DataAccess.TableExists(Options.Table.Name)) return false;
        _checkTableLog = true;
        return true;

    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {

        if (!IsEnabled(logLevel))
        {
            return;
        }

        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
            case LogLevel.Information:
            case LogLevel.None:
                AddInfo(formatter(state,exception));
                break;
            case LogLevel.Warning:
                AddWarning(formatter(state, exception));
                break;
            case LogLevel.Error:
            case LogLevel.Critical:
                AddError(formatter(state, exception));
                break;
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
            case LogLevel.Information:
            case LogLevel.Warning:
            case LogLevel.Error:
            case LogLevel.Critical:
                //TODO: Create LogLevel parametrization for JJMasterData's Logger.
                return true;
            case LogLevel.None:
            default:
                return false;
        }
    }

    public IDisposable BeginScope<TState>(TState state) => default!;
}
