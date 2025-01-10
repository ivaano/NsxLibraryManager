function getCursorLocation(field, myValue) {
    let input = document.getElementById(field);
    return input.selectionStart;    
}

function setFocus(field) {
    let input = document.getElementById(field);
    input.focus();
}

function scrollDialogToTop() {
    const dialogContent = document.querySelector('.rz-dialog-content');
    if (dialogContent) {
        dialogContent.scrollTop = 0;
    }
}