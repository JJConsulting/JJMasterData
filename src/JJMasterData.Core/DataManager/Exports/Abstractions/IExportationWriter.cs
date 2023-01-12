using System.Collections;
using System.Data;
using System.IO;
using System.Threading;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.DataManager.Exports.Abstractions;

public interface IExportationWriter : IBackgroundTaskWorker
{
    public FormElement FormElement { get; set; }
    
    public FieldManager FieldManager { get; set; }
    
    public ExportOptions Configuration { get; set; }
    
    public string UserId { get; set; }
    
    public ProcessOptions ProcessOptions { get; set; }
    
    public DataTable DataSource { get; set; }
    
    public IHttpContext CurrentContext { get; internal set; }
    
    public string AbsoluteUri { get; set; }
    
    public string FolderPath { get; }
    
    public Hashtable CurrentFilter { get; set; }
    
    public string CurrentOrder { get; set; }
    
    void GenerateDocument(Stream stream, CancellationToken token);
}