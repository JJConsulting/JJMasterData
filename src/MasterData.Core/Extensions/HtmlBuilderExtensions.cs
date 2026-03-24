using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;

namespace JJMasterData.Core.Extensions;

internal static class HtmlBuilderExtensions
{
    extension(HtmlBuilder htmlBuilder)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HtmlBuilder AppendText(char @char)
        {
            return htmlBuilder.AppendText(@char.ToString());
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<HtmlBuilder> AsValueTask()
        {
            return new ValueTask<HtmlBuilder>(htmlBuilder);
        }
    }
}