var messageWait = (function () {
    var htmlIdWaitModal = "#jjform_wait_message";

    function loadHtml() {

        if (!$(htmlIdWaitModal).length) {

            var html = "";
            html += "<div id=\"jjform_wait_message\">\r\n";
            html += "    <div class=\"ajaxImage\"></div>\r\n";
            html += "    <div class=\"ajaxMessage\">Aguarde...</div>\r\n";
            html += "</div>";

            $(html).insertAfter($("body"));

            var opts = {
                lines: 17, // The number of lines to draw
                length: 28, // The length of each line
                width: 14, // The line thickness
                radius: 38, // The radius of the inner circle
                scale: 0.40, // Scales overall size of the spinner
                corners: 1, // Corner roundness (0..1)
                color: "#000", // #rgb or #rrggbb or array of colors
                opacity: 0.3, // Opacity of the lines
                rotate: 0, // The rotation offset
                direction: 1, // 1: clockwise, -1: counterclockwise
                speed: 1.2, // Rounds per second
                trail: 62, // Afterglow percentage
                fps: 20, // Frames per second when using setTimeout() as a fallback for CSS
                zIndex: 2e9, // The z-index (defaults to 2000000000)
                className: "spinner", // The CSS class to assign to the spinner
                top: "50%", // Top position relative to parent
                left: "50%", // Left position relative to parent
                shadow: false, // Whether to render a shadow
                hwaccel: false, // Whether to use hardware acceleration
                position: "absolute", // Element positioning
            };

            // @ts-ignore
            var spinner = new Spinner(opts).spin();

            $(spinner.el).insertAfter($("#jjform_wait_message .ajaxImage"));
        }


    }

    return {

        /**
         * Exibe uma caixa de mensagem aguarde...
         */
        show: function () {
            loadHtml();
            $(htmlIdWaitModal).css("display", "");
        },

        /**
         * Oculta a caixa de mensagem aguarde
         */
        hide: function () {
            $(htmlIdWaitModal).css("display", "none");
        }

    };
})();