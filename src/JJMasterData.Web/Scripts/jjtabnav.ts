class JJTabNav{
    static setup(){
        $("a.jj-tab-link").on("shown.bs.tab", function (e) {
            var link = $(e.target);
            $("#" + link.attr("jj-objectid")).val(link.attr("jj-tabindex"));
        });
    }
}