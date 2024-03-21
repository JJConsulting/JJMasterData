using JetBrains.Annotations;

namespace JJMasterData.Core.Http.Abstractions;

public interface IMasterDataUrlHelper
{
#if NET
    string Action([AspMvcAction] string action = null, 
                  [AspMvcController] string controller = null, 
                   [AspMvcArea("Area"),
                   AspMvcModelType] object values = null);
    
    string Content(string contentPath);
#else
    string Action(string action = null, 
                  string controller = null, 
                  object values = null);
#endif    
}