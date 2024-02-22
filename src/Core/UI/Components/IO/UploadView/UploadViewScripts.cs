using System.Web;

namespace JJMasterData.Core.UI.Components;

public class UploadViewScripts(JJUploadView uploadView)
{
    private string JsCallBack => HttpUtility.JavaScriptStringEncode(uploadView.JsCallback);

    public string GetDeleteFileScript()
    {
        string message = uploadView.StringLocalizer["Would you like to delete this record?"];
        //language=Javascript
        return $"UploadViewHelper.deleteFile('{uploadView.Name}','{{NameJS}}','{message}','{JsCallBack}');";
    }
    
    public string GetDownloadFileScript()
    {
        //language=Javascript
        return $"UploadViewHelper.downloadFile('{uploadView.Name}','{{NameJS}}', '{JsCallBack}');";
    }
    
    public string GetRenameFileScript()
    {
        string message = uploadView.StringLocalizer["Enter the new name for the file:"];
        //language=Javascript
        return $"UploadViewHelper.renameFile('{uploadView.Name}','{{NameJS}}','{message}','{JsCallBack}');";
    }
}