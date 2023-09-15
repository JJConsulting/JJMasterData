using System.Web;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.UI.Components.IO.UploadView;

public class UploadViewScripts
{
    private readonly JJUploadView _uploadView;

    private string JsCallBack => HttpUtility.JavaScriptStringEncode(_uploadView.JsCallback);
    
    public UploadViewScripts(JJUploadView uploadView)
    {
        _uploadView = uploadView;
    }

    public string GetDeleteFileScript()
    {
        string message = _uploadView.StringLocalizer["Would you like to delete this record?"];
        return $"UploadViewHelper.deleteFile('{_uploadView.Name}','{{NameJS}}','{message}','{JsCallBack}');";
    }
    
    public string GetDownloadFileScript()
    {
        return $"UploadViewHelper.downloadFile('{_uploadView.Name}','{{NameJS}}', '{JsCallBack}');";
    }
    
    public string GetRenameFileScript()
    {
        string message = _uploadView.StringLocalizer["Enter the new name for the file:"];
        return $"UploadViewHelper.renameFile('{_uploadView.Name}','{{NameJS}}','{message}','{JsCallBack}');";
    }
}