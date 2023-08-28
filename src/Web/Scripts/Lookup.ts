class Lookup {
    static setup(){
        $("input.jjlookup").each(function () {
            let lookupInput = $(this);
            let lookupId = lookupInput.attr("id");
            let fieldName = lookupInput.attr("lookup-field-name");
            let panelName = lookupInput.attr("panelName");
            let popupTitle = lookupInput.attr("popuptitle");
            let lookupUrl = lookupInput.attr("lookup-url");
            let lookupResultUrl = lookupInput.attr("lookup-result-url");
            let dataPanelReloadUrl = lookupInput.attr("data-panel-reload-url");
            let popupSize : number = +lookupInput.attr("popupsize");
            let form =document.querySelector<HTMLFormElement>("form");

            const jjLookupSelector = "#" + lookupId + "";
            const jjHiddenLookupSelector = "#id_" + lookupId + "";
            
            $("#btn_" + lookupId).on("click",function () {
                popup.show(popupTitle, lookupUrl, popupSize);
            });

            function setHiddenLookup(){
                $("#id_" + lookupId).val( lookupInput.val())
            }

            lookupInput.one("focus",function () {
                lookupInput.val($("#id_" + lookupId).val()).select();
            });

            lookupInput.one("change",function () {
                $("#id_" + lookupId).val(lookupInput.val());
            });
            
            lookupInput.one("blur",function () {
                showWaitOnPost = false;
                setHiddenLookup();
                
                FeedbackIcon.removeAllIcons(jjLookupSelector)

                lookupInput.removeAttr("readonly");
                if (lookupInput.val() == "") {
                    return;
                }
                

                if(!lookupResultUrl){
                    let urlBuilder = new UrlBuilder()
                    urlBuilder.addQueryParameter("lookup-" + panelName, fieldName)
                    urlBuilder.addQueryParameter("lookupAction","getDescription")
                    urlBuilder.addQueryParameter("lkid",lookupInput.val().toString())
                    lookupResultUrl = urlBuilder.build()
                }

                lookupInput.addClass("loading-circle");
                const formData = new FormData(form);
                fetch(lookupResultUrl, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'Accept': 'application/json',
                    },
                })
                    .then(response => response.json())
                    .then(data => {
                        showWaitOnPost = true;
                        lookupInput.removeClass("loading-circle");
                        if (data.description === "") {
                            FeedbackIcon.setIcon(jjLookupSelector, FeedbackIcon.warningClass);
                        } else {
                            const lookupHiddenInputElement =  document.querySelector<HTMLInputElement>("#id_" + lookupId);
                            const lookupInputElement = document.querySelector<HTMLInputElement>("#" + lookupId);
                            FeedbackIcon.setIcon(jjLookupSelector, FeedbackIcon.successClass);
                            lookupInputElement.value = data.description;
                            lookupHiddenInputElement.value = data.id;
                            
                            if(dataPanelReloadUrl){
                                DataPanel.reload(dataPanelReloadUrl,panelName,lookupId)
                            }
                            else{
                                DataPanel.reloadAtSamePage(panelName, lookupId);
                            }
         
                        }
                    })
                    .catch(error => {
                        showWaitOnPost = true;
                        lookupInput.removeClass("loading-circle");
                        FeedbackIcon.setIcon(jjLookupSelector, FeedbackIcon.errorClass);
                        console.log(error);
                    });

            });
        });
    }
}