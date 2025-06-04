#nullable enable
using System.Threading.Tasks;

namespace JJMasterData.Commons.Tasks;

public delegate ValueTask AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e);
