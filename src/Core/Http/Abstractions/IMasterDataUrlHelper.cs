using JetBrains.Annotations;

namespace JJMasterData.Core.Http.Abstractions;

public interface IMasterDataUrlHelper
{
    string Action([AspMvcAction] string action = null, [AspMvcController] string controller = null, object values = null);
}