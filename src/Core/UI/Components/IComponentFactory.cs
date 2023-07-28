using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.UI.Components;

public interface IComponentFactory
{
    
}

public interface IComponentFactory<TComponent> : IComponentFactory where TComponent : JJBaseView
{
    TComponent Create();
}