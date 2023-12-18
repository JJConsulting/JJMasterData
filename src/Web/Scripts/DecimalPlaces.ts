function applyDecimalPlaces(element: Element) {
    if(element.getAttribute("type") == "number")
        return;
    
    let decimalPlaces = element.getAttribute("jj-decimal-places") ?? 2;
    let decimalSeparator = element.getAttribute("jj-decimal-separator") ?? '.';
    let groupSeparator = element.getAttribute("jj-group-separator") ?? ',';
    
    // @ts-ignore
    new AutoNumeric(element, {
        decimalCharacter: decimalSeparator,
        digitGroupSeparator: groupSeparator,
        decimalPlaces: decimalPlaces
    });

}