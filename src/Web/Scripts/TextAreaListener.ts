class TextAreaListener {
    static listenKeydown(selectorPrefix: string) {
        $(selectorPrefix + "textarea").on("input", function () {
            TextAreaListener.handleInputOrPaste($(this));
        });

        $(selectorPrefix + "textarea").on("paste", async function () {
            const jjTextArea = $(this);
       
            setTimeout(function () {
                TextAreaListener.handleInputOrPaste(jjTextArea);
            }, 0);
        });
    }

    private static getTextAreaVal(jjTextArea){
        return jjTextArea.val().replace( /\r?\n/g, "\r\n" );
    }
    
    private static handleInputOrPaste(jjTextArea) {
        const maxLength = parseInt(jjTextArea.attr("maxlength") || jjTextArea.attr("jjmaxlength"), 10);
        
        if(!maxLength)
            return;
        
        const maximumLimitLabel = jjTextArea.attr("maximum-limit-of-characters-label") || "Maximum limit of {0} characters!";
        const charactersRemainingLabelTemplate = jjTextArea.attr("characters-remaining-label") || "({0} characters remaining)";

        let typedChars = this.getTextAreaVal(jjTextArea).toString().length;
        if (typedChars > maxLength) {
            alert(maximumLimitLabel.replace("{0}", maxLength.toString()));
            jjTextArea.val(this.getTextAreaVal(jjTextArea).toString().substring(0, maxLength));
            typedChars = this.getTextAreaVal(jjTextArea).toString().length;
        }
        
        const charactersRemaining = maxLength - typedChars;
        const charactersRemainingLabel = charactersRemainingLabelTemplate.replace("{0}", charactersRemaining.toString()) + "&nbsp;";

        const spanId = `#span-size-${jjTextArea.attr("id")}`;
        if ($(spanId).length) {
            $(spanId).html(charactersRemainingLabel);
        } else {
            $("<span>", {
                id: `span-size-${jjTextArea.attr("id")}`,
                class: "small",
                style: "float: right",
                html: charactersRemainingLabel
            }).insertBefore(jjTextArea);
        }
    }
}