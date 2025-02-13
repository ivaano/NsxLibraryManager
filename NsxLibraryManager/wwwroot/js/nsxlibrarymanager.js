function getCursorLocation(field, myValue) {
    let input = document.getElementById(field);
    return input.selectionStart;    
}

function setFocus(field) {
    let input = document.getElementById(field);
    input.focus();
}

window.deleteAllCookies = function () {
    let cookies = document.cookie.split(";");

    for (let i = 0; i < cookies.length; i++) {
        let cookie = cookies[i];
        let equalsPos = cookie.indexOf("=");
        let name = equalsPos > -1 ? cookie.substr(0, equalsPos) : cookie;
        document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    }
}

