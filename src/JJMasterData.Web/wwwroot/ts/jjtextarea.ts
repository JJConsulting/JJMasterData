class JJTextArea{
    static setup(){
        $("textarea").keydown(function () {
            var oTextArea = $(this);
            var iMaxChar : any = oTextArea.attr("maxlength");
            var strValid : any = oTextArea.attr("strvalid2");
            var strChars : any = oTextArea.attr("strchars");

            if (isNaN(iMaxChar))
                iMaxChar = oTextArea.attr("jjmaxlength");

            if (isNaN(iMaxChar))
                return;

            if (isNaN(strValid))
                strValid = "Maximum limit of {0} characters!";

            if (isNaN(strChars))
                strChars = "({0} characters remaining)";

            if (!isNaN(iMaxChar)) {
                var nId = oTextArea.attr("id");
                var iTypedChar : number = oTextArea.val().toString().length;
                if (iTypedChar > iMaxChar) {
                    alert(strValid.replace("{0}", iMaxChar));
                }

                strChars = strChars.replace("{0}", (iMaxChar - oTextArea.val().toString().length));
                strChars += "&nbsp;";

                if ($("#spansize_" + nId).length) {
                    $("#spansize_" + nId).html(strChars);
                } else {
                    $("<span id='spansize_" + nId + "' class='small' style='float: right'>" + strChars + "</span>").insertBefore(oTextArea);
                }
            }
        });
    }
}