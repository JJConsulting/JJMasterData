using System.Threading.Tasks;
using JJMasterData.Core.UI.Events.Args;

namespace JJMasterData.Core.UI.Events;

public delegate Task GridDataLoadEventHandler(object sender, GridDataLoadEventArgs e);
