class JJDataImp {
    private static insertCount = 0;
    private static updateCount = 0;
    private static deleteCount = 0;
    private static ignoreCount = 0;
    private static errorCount = 0;

    private static setLoadMessage() {
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

    private static checkProcess(objname) {
        showWaitOnPost = false;
        const form = $("form");
        let url: string = form.attr("action");
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
                    if (result.PercentProcess == 100)
                        $("#lblInsertCount").text(result.Insert);
                    else
                        jjutil.animateValue("lblInsertCount", JJDataImp.insertCount, result.Insert, 1000);

                    JJDataImp.insertCount = result.Insert;
                }

                if (result.Update > 0) {
                    $("#lblUpdate").css("display", "");
                    if (result.PercentProcess == 100)
                        $("#lblUpdateCount").text(result.Update);
                    else
                        jjutil.animateValue("lblUpdateCount", JJDataImp.updateCount, result.Update, 1000);

                    JJDataImp.updateCount = result.Update;
                }

                if (result.Delete > 0) {
                    $("#lblDelete").css("display", "");
                    if (result.PercentProcess == 100)
                        $("#lblDeleteCount").text(result.Delete);
                    else
                        jjutil.animateValue("lblDeleteCount", JJDataImp.deleteCount, result.Delete, 1000);

                    JJDataImp.deleteCount = result.Delete;
                }

                if (result.Ignore > 0) {
                    $("#lblIgnore").css("display", "");
                    if (result.PercentProcess == 100)
                        $("#lblIgnoreCount").text(result.Ignore);
                    else
                        jjutil.animateValue("lblIgnoreCount", JJDataImp.ignoreCount, result.Ignore, 1000);

                    JJDataImp.ignoreCount = result.Ignore;
                }

                if (result.Error > 0) {
                    $("#lblError").css("display", "");
                    if (result.PercentProcess == 100)
                        $("#lblErrorCount").text(result.Error);
                    else
                        jjutil.animateValue("lblErrorCount", JJDataImp.errorCount, result.Error, 1000);

                    JJDataImp.errorCount = result.Error;
                }

                if (!result.IsProcessing) {
                    $("#current_uploadaction").val("process_finished");
                    setTimeout(function () {
                        $("form:first").submit()
                    }, 1000);
                }
            }
        });
    }

    static startProcess(objname) {
        $(document).ready(function () {
            JJDataImp.setLoadMessage();

            setInterval(function () {
                JJDataImp.checkProcess(objname);
            }, 3000);
        });
    }

    static stopProcess(objname, stopStr) {
        $("#divMsgProcess").html(stopStr);
        showWaitOnPost = false;

        const form = $("form");

        let url: string = form.attr("action");

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

    static addPasteListener() {
        $(document).ready(function () {
            document.addEventListener("paste", (e: Event) => {
                var pastedText = undefined;
                if (window.clipboardData && window.clipboardData.getData) { // IE 
                    pastedText = window.clipboardData.getData("Text");
                } else if (e.clipboardData && e.clipboardData.getData) {
                    pastedText = e.clipboardData.getData("text/plain");
                }
                e.preventDefault();
                if (pastedText != undefined) {

                    $("#current_uploadaction").val("posted_past_text");
                    $("#pasteValue").val(pastedText);
                    $("form:first").submit();
                }
                return false;
            });
        });
    }

}
