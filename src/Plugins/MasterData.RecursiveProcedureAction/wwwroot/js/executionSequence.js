function setExecutionSequence(executionSequence){
    const form = getMasterDataForm();

    const existingInput = form.querySelector("input[name='ExecutionSequence']");

    if(executionSequence === -1){
        const obs = document.getElementById("ExecutionSequenceObs");
        if(obs){
            obs.value = "";
        }
    }
    
    if (!existingInput) {
        const hiddenInput = document.createElement("input");
        hiddenInput.type = "hidden";
        hiddenInput.name = "ExecutionSequence";
        hiddenInput.value = executionSequence;

        form.appendChild(hiddenInput);
    } else{
        existingInput.value = executionSequence;
    }
}

function hideDialogs(id) {
    bootstrap.Modal.getOrCreateInstance('#' + id).hide();
    
    const inIframe = window.self !== window.top;
    
    if (inIframe) {
        window.parent.getMasterDataForm().submit();
        window.parent.defaultModal.hide();
    }
}