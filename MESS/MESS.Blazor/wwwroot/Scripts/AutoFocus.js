var AutoFocus = AutoFocus || {};

AutoFocus.setFocus = function (id) {
    var element = document.getElementById(id)
    if (element) {
        element.focus()
    }
}