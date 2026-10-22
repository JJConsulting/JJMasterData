using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace JJConsulting.Html;

public sealed class HtmlText(string rawText, bool encode = true) : IHtmlContent
{
    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        writer.Write(encode ? encoder.Encode(rawText) : rawText);
    }
}