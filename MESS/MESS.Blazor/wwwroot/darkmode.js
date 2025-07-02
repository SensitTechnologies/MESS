document.addEventListener('DOMContentLoaded', () => {
    const isDarkMode = localStorage.getItem('darkMode') === 'true';
    toggleDarkMode(isDarkMode);
});

function toggleDarkMode(isDarkMode) {
    document.body.className = isDarkMode ? 'dark-mode' : 'light-mode';
    localStorage.setItem('darkMode', isDarkMode);
}

window.FixDarkModeFailureTextArea = function () {
    if (!document.body.classList.contains("dark-mode")) return;

    const fluentTextAreas = document.querySelectorAll("fluent-text-area.failure-textarea");

    fluentTextAreas.forEach(fluentTextArea => {
        // Style the host element (the outer tag)
        fluentTextArea.style.backgroundColor = "#2c2c2c";
        fluentTextArea.style.borderRadius = "4px";
        fluentTextArea.style.display = "inline-block"; // ensure size fits content

        const shadowRoot = fluentTextArea.shadowRoot;
        if (!shadowRoot) return;

        // Style the part="control" wrapper (if it exists)
        const control = shadowRoot.querySelector('[part="control"]');
        if (control) {
            control.style.backgroundColor = "#2c2c2c";
            control.style.borderColor = "#555";
            control.style.borderRadius = "4px";
            control.style.padding = "0.5rem";
        }

        // Style the actual <textarea>
        const textarea = shadowRoot.querySelector("textarea");
        if (textarea) {
            textarea.style.backgroundColor = "#2c2c2c";
            textarea.style.color = "white";
            textarea.style.caretColor = "white";
            textarea.style.border = "none";
            textarea.style.padding = "0.5rem";
            textarea.style.fontSize = "1rem";
            textarea.style.resize = "vertical";
        }
    });

    window.ApplyDarkModeFixToFailureTextAreas = function () {
        document.querySelectorAll("fluent-text-area.failure-textarea").forEach(el => {
            const shadow = el.shadowRoot;
            if (!shadow) return;

            const textarea = shadow.querySelector("textarea");

            if (textarea) {
                textarea.style.backgroundColor = "#2c2c2c";
                textarea.style.color = "white";
                textarea.style.border = "none";
                textarea.style.padding = "0.5rem";
                textarea.style.borderRadius = "4px";
                textarea.style.resize = "vertical";
                textarea.style.minHeight = "100px";
                textarea.style.maxHeight = "300px";
                textarea.style.fontSize = "1rem";
                textarea.style.caretColor = "white";
            }
        });
    };
};
