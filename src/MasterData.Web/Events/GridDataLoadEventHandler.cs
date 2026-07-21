using System.Threading.Tasks;
using JJMasterData.Web.Events.Args;

namespace JJMasterData.Web.Events;

public delegate Task GridDataLoadEventHandler(object sender, GridDataLoadEventArgs e);
