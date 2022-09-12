class JJDataImp{
    private static setLoadMessage(){
        const options = {
            lines: 13, // The number of lines to draw
            length: 38, // The length of each line
            width: 17, // The line thickness
            radius: 45, // The radius of the inner circle
            scale: 0.2, // Scales overall size of the spinner
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
            position: "absolute" // Element positioning

        };
        const target = document.getElementById('impSpin');
        // @ts-ignore
        new Spinner(options).spin(target);
    }
    
    private static checkProcess(objname){
        showWaitOnPost = false;
        const form = $("form");
        let url : string = form.attr("action");
        if (url.includes("?"))
            url += "&t=ajaxdataimp&current_uploadaction=process_check&objname=" + objname;
        else
            url += "?t=ajaxdataimp&current_uploadaction=process_check&objname=" + objname;

        $.ajax({
            url: url,
            dataType: "json",
            cache: false,
            success: function (result) {

                $("#divMsgProcess").css("display", "");
                $(".progress-bar").css("width", result.PercentProcess + "%")
                    .text(result.PercentProcess + "%");

                $("#lblResumeLog").text(result.Message);
                $("#lblStartDate").text(result.StartDate);

                if (result.Insert > 0) {
                    $("#lblInsert").css("display", "");
                    $("#lblInsertCount").text(result.Insert);
                }

                if (result.Update > 0) {
                    $("#lblUpdate").css("display", "");
                    $("#lblUpdateCount").text(result.Update);
                }

                if (result.Delete > 0) {
                    $("#lblDelete").css("display", "");
                    $("#lblDeleteCount").text(result.Delete);
                }

                if (result.Ignore > 0) {
                    $("#lblIgnore").css("display", "");
                    $("#lblIgnoreCount").text(result.Ignore);
                }

                if (result.Error > 0) {
                    $("#lblError").css("display", "");
                    $("#lblErrorCount").text(result.Error);
                }

                if (!result.IsProcessing) {
                    $("#current_uploadaction").val("process_finished");
                    $("form:first").submit();
                }
            }
        });
    }
    
    static startProcess(objname){
        this.setLoadMessage();

        setInterval(function () {
            JJDataImp.checkProcess(objname);
        }, (3 * 1000));
    }
    
    static stopProcess(objname, stopStr) {
        $("#divMsgProcess").html(stopStr);
        showWaitOnPost = false;

        const form = $("form");

        let url : string = form.attr("action");

        if (url.includes("?"))
            url += "&t=ajaxdataimp&current_uploadaction=process_stop&objname=" + objname;
        else
            url += "?t=ajaxdataimp&current_uploadaction=process_stop&objname=" + objname;

        $.ajax({
            url: url,
            dataType: "json",
            cache: false
        });
    }
}