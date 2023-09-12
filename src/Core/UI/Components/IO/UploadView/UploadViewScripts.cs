using System.Web;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.UI.Components.IO.UploadView;

public class UploadViewScripts
{
    private readonly JJUploadView _uploadView;

    public UploadViewScripts(JJUploadView uploadView)
    {
        _uploadView = uploadView;
    }

    public string GetDeleteFileScript()
    {
        return
            $"UploadViewHelper.deleteFile('{_uploadView.Name}','{{NameJS}}','{_uploadView.StringLocalizer["Would you like to delete this record?"]}','{HttpUtility.JavaScriptStringEncode(_uploadView.JsCallback)}');";
    }
    
    public string GetDownloadFileScript()
    {
        return
            $"UploadViewHelper.downloadFile('{_uploadView.Name}','{{NameJS}}');";
    }
    
    public string GetRenameFileScript()
    {
        return
            $"UploadViewHelper.renameFile('{_uploadView.Name}','{{NameJS}}','{_uploadView.StringLocalizer["Enter the new name for the file:"]}','{HttpUtility.JavaScriptStringEncode(_uploadView.JsCallback)}');";
    }
}