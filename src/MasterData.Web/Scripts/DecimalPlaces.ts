function applyDecimalPlaces(element: Element) {

    // @ts-ignore
    if(AutoNumeric.getAutoNumericElement(element) !== null)
        return;
    
    if(element.getAttribute("type") == "number")
        return;
    
    const decimalPlaces = element.getAttribute("jj-decimal-places") ?? 2;
    const decimalSeparator = element.getAttribute("jj-decimal-separator") ?? '.';
    const groupSeparator = element.getAttribute("jj-group-separator") ?? ',';
    
    // @ts-ignore
    new AutoNumeric(element, {
        decimalCharacter: decimalSeparator,
        digitGroupSeparator: groupSeparator,
        decimalPlaces: decimalPlaces
    });

}