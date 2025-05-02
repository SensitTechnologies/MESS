window.focusNotesField = function () {
    const notesField = document.querySelector("[data-testid='optional-notes']");
    if (notesField) {
        notesField.focus();
    }
};
