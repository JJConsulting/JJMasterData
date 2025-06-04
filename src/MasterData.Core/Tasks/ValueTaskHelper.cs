using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace JJMasterData.Core.Tasks;

internal static class ValueTaskHelper
{
    public static ValueTask CompletedTask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
#if NET
            return ValueTask.CompletedTask;
#else
            return default;
#endif
        }
    }
}