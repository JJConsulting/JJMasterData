class TextAreaListener {
    static listenKeydown(selectorPrefix){
        $(selectorPrefix + "textarea").keydown(function () {
            const jjTextArea = $(this);
            let maxLength : any = jjTextArea.attr("maxlength");
            let maximumLimitLabel : any = jjTextArea.attr("maximum-limit-of-characters-label");
            let charactersRemainingLabel : any = jjTextArea.attr("characters-remaining-label");

            if (isNaN(maxLength))
                maxLength = jjTextArea.attr("jjmaxlength");

            if (isNaN(maxLength))
                return;

            if (isNaN(maximumLimitLabel))
                maximumLimitLabel = "Maximum limit of {0} characters!";

            if (isNaN(charactersRemainingLabel))
                charactersRemainingLabel = "({0} characters remaining)";

            if (!isNaN(maxLength)) {
                var nId = jjTextArea.attr("id");
                var iTypedChar : number = jjTextArea.val().toString().length;
                if (iTypedChar > maxLength) {
                    alert(maximumLimitLabel.replace("{0}", maxLength));
                }

                charactersRemainingLabel = charactersRemainingLabel.replace("{0}", (maxLength - jjTextArea.val().toString().length));
                charactersRemainingLabel += "&nbsp;";

                if ($("#span-size-" + nId).length) {
                    $("#span-size-" + nId).html(charactersRemainingLabel);
                } else {
                    $("<span id='span-size-" + nId + "' class='small' style='float: right'>" + charactersRemainingLabel + "</span>").insertBefore(jjTextArea);
                }
            }
        });
    }
}