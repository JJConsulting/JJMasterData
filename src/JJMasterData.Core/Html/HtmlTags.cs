namespace JJMasterData.Core.Html;

/// <summary>
/// Static provider class that gives access to all HTML 5 tags.
/// </summary>
public static class HtmlTags
{
#pragma warning disable SA1600 // Elements should be documented

    // Structural Tags
    public static HtmlTag A => new HtmlTag("a");

    public static HtmlTag Article => new HtmlTag("article");

    public static HtmlTag Aside => new HtmlTag("aside");

    public static HtmlTag Body => new HtmlTag("body");

    public static HtmlTag Br => new HtmlTag("br", false);

    public static HtmlTag Details => new HtmlTag("details");

    public static HtmlTag Div => new HtmlTag("div");

    public static HtmlTag H1 => new HtmlTag("h1");

    public static HtmlTag H2 => new HtmlTag("h2");

    public static HtmlTag H3 => new HtmlTag("h3");

    public static HtmlTag H4 => new HtmlTag("h4");

    public static HtmlTag H5 => new HtmlTag("h5");

    public static HtmlTag H6 => new HtmlTag("h6");

    public static HtmlTag Head => new HtmlTag("head");

    public static HtmlTag Header => new HtmlTag("header");

    public static HtmlTag Hgroup => new HtmlTag("hgroup");

    public static HtmlTag Hr => new HtmlTag("hr", false);

    public static HtmlTag Html => new HtmlTag("html");

    public static HtmlTag Footer => new HtmlTag("footer");

    public static HtmlTag Nav => new HtmlTag("nav");

    public static HtmlTag P => new HtmlTag("p");

    public static HtmlTag Section => new HtmlTag("section");

    public static HtmlTag Span => new HtmlTag("span");

    public static HtmlTag Summary => new HtmlTag("summary");

    // Metadata Tags
    public static HtmlTag Base => new HtmlTag("base", false);

    public static HtmlTag Link => new HtmlTag("link", false);

    public static HtmlTag Meta => new HtmlTag("meta", false);

    public static HtmlTag Style => new HtmlTag("style");

    public static HtmlTag Title => new HtmlTag("title");

    // Form Tags
    public static HtmlTag Button => new HtmlTag("button");

    public static HtmlTag Datalist => new HtmlTag("datalist");

    public static HtmlTag Fieldset => new HtmlTag("fieldset");

    public static HtmlTag Form => new HtmlTag("form");

    public static HtmlTag Input => new HtmlTag("input", false);

    public static HtmlTag Keygen => new HtmlTag("keygen", false);

    public static HtmlTag Label => new HtmlTag("label");

    public static HtmlTag Legend => new HtmlTag("legend");

    public static HtmlTag Meter => new HtmlTag("meter");

    public static HtmlTag Optgroup => new HtmlTag("optgroup");

    public static HtmlTag Option => new HtmlTag("option");

    public static HtmlTag Select => new HtmlTag("select");

    public static HtmlTag Textarea => new HtmlTag("textarea");

    // Formatting Tags
    public static HtmlTag Abbr => new HtmlTag("abbr");

    public static HtmlTag Address => new HtmlTag("address");

    public static HtmlTag B => new HtmlTag("b");

    public static HtmlTag Bdi => new HtmlTag("bdi");

    public static HtmlTag Bdo => new HtmlTag("bdo");

    public static HtmlTag Blockquote => new HtmlTag("blockquote");

    public static HtmlTag Cite => new HtmlTag("cite");

    public static HtmlTag Code => new HtmlTag("code");

    public static HtmlTag Del => new HtmlTag("del");

    public static HtmlTag Dfn => new HtmlTag("dfn");

    public static HtmlTag Em => new HtmlTag("em");

    public static HtmlTag I => new HtmlTag("i");

    public static HtmlTag Ins => new HtmlTag("ins");

    public static HtmlTag Kbd => new HtmlTag("kbd");

    public static HtmlTag Mark => new HtmlTag("mark");

    public static HtmlTag Output => new HtmlTag("output");

    public static HtmlTag Pre => new HtmlTag("pre");

    public static HtmlTag Progress => new HtmlTag("progress");

    public static HtmlTag Q => new HtmlTag("q");

    public static HtmlTag Rp => new HtmlTag("rp");

    public static HtmlTag Rt => new HtmlTag("rt");

    public static HtmlTag Ruby => new HtmlTag("ruby");

    public static HtmlTag Samp => new HtmlTag("samp");

    public static HtmlTag Small => new HtmlTag("small");

    public static HtmlTag Strong => new HtmlTag("strong");

    public static HtmlTag Sub => new HtmlTag("sub");

    public static HtmlTag Sup => new HtmlTag("sup");

    public static HtmlTag Var => new HtmlTag("var");

    public static HtmlTag Wbr => new HtmlTag("wbr");

    // List Tags
    public static HtmlTag Dd => new HtmlTag("dd");

    public static HtmlTag Dl => new HtmlTag("dl");

    public static HtmlTag Dt => new HtmlTag("dt");

    public static HtmlTag Li => new HtmlTag("li");

    public static HtmlTag Ol => new HtmlTag("ol");

    public static HtmlTag Menu => new HtmlTag("menu");

    public static HtmlTag Ul => new HtmlTag("ul");

    // Table Tags
    public static HtmlTag Caption => new HtmlTag("caption");

    public static HtmlTag Col => new HtmlTag("col", false);

    public static HtmlTag Colgroup => new HtmlTag("colgroup");

    public static HtmlTag Table => new HtmlTag("table");

    public static HtmlTag Tbody => new HtmlTag("tbody");

    public static HtmlTag Td => new HtmlTag("td");

    public static HtmlTag Tfoot => new HtmlTag("tfoot");

    public static HtmlTag Thead => new HtmlTag("thead");

    public static HtmlTag Th => new HtmlTag("th");

    public static HtmlTag Tr => new HtmlTag("tr");

    // Scripting Tags
    public static HtmlTag Noscript => new HtmlTag("noscript");

    public static HtmlTag Script => new HtmlTag("script");

    // Embedded Content Tags
    public static HtmlTag Area => new HtmlTag("area", false);

    public static HtmlTag Audio => new HtmlTag("audio");

    public static HtmlTag Canvas => new HtmlTag("canvas");

    public static HtmlTag Embed => new HtmlTag("embed", false);

    public static HtmlTag Figcaption => new HtmlTag("figcaption");

    public static HtmlTag Figure => new HtmlTag("figure");

    public static HtmlTag Iframe => new HtmlTag("iframe");

    public static HtmlTag Img => new HtmlTag("img", false);

    public static HtmlTag Map => new HtmlTag("map");

    public static HtmlTag Object => new HtmlTag("object");

    public static HtmlTag Param => new HtmlTag("param", false);

    public static HtmlTag Source => new HtmlTag("source", false);

    public static HtmlTag Time => new HtmlTag("time");

    public static HtmlTag Video => new HtmlTag("video");

#pragma warning restore SA1600 // Elements should be documented
}
