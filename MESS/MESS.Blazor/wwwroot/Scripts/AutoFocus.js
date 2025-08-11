var AutoFocus = AutoFocus || {};

AutoFocus.setFocus = function (id) {
    var element = document.getElementById(id)
    if (element) {
        element.focus()
    }
}

AutoFocus.selectAll = function (element) {
    if (element && typeof element.select === "function") {
        element.select();
    }
};