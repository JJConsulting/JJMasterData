function applyDecimalPlaces() {
    let decimalPlaces = $(this).attr("jjdecimalplaces");
    if (decimalPlaces == null)
        decimalPlaces = "2";

    if(localeCode==='pt')
        $(this).number(true, decimalPlaces, ",", ".");
    else
        $(this).number(true, decimalPlaces);
    
}