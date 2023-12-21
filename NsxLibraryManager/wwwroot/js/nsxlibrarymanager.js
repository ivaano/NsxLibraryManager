function getCursorLocation(field, myValue) {
    let input = document.getElementById(field);
    return input.selectionStart;    
}

function setFocus(field) {
    let input = document.getElementById(field);
    input.focus();
}