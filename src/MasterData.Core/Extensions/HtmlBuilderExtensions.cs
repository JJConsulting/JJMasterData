using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JJConsulting.Html;

namespace JJMasterData.Core.Extensions;

internal static class HtmlBuilderExtensions
{
    extension(HtmlBuilder htmlBuilder)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<HtmlBuilder> AsValueTask()
        {
            return new ValueTask<HtmlBuilder>(htmlBuilder);
        }
    }
}