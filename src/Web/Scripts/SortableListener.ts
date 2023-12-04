class SortableListener {
    static listenSorting(selectorPrefix = String()){
        ($(selectorPrefix + ".jjsortable") as any).sortable({
            helper: function (e, tr) {
                var originals = tr.children();
                var helper = tr.clone();
                helper.children().each(function (index) {
                    // Set helper cell sizes to match the original sizes
                    $(this).width(originals.eq(index).width());
                });
                return helper;
            },
            change: function (event, ui) {
                ui.placeholder.css({
                    visibility: "visible",
                    background: "#fbfbfb"
                });
            }
        });
    }
}