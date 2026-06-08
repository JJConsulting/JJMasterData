using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using Microsoft.AspNetCore.Html;

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
        
        public HtmlBuilder AppendRawHtml(string html)
        {
            return htmlBuilder.Append(new HtmlBuilder(html, encode:false));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<HtmlBuilder> AsValueTask()
        {
            return new ValueTask<HtmlBuilder>(htmlBuilder);
        }
    }
}