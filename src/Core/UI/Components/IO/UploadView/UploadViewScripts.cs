using System.Web;

namespace JJMasterData.Core.UI.Components;

public class UploadViewScripts(JJUploadView uploadView)
{
    private string JsCallBack => HttpUtility.JavaScriptStringEncode(uploadView.JsCallback);

    public string GetDeleteFileScript()
    {
        string message = uploadView.StringLocalizer["Would you like to delete this record?"];
        return $"UploadViewHelper.deleteFile('{uploadView.Name}','{{NameJS}}','{message}','{JsCallBack}');";
    }
    
    public string GetDownloadFileScript()
    {
        return $"UploadViewHelper.downloadFile('{uploadView.Name}','{{NameJS}}', '{JsCallBack}');";
    }
    
    public string GetRenameFileScript()
    {
        string message = uploadView.StringLocalizer["Enter the new name for the file:"];
        return $"UploadViewHelper.renameFile('{uploadView.Name}','{{NameJS}}','{message}','{JsCallBack}');";
    }
}