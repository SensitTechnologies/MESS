function applyDarkModeToFluentTextAreas(isDark) {
    document.querySelectorAll("fluent-text-area").forEach(el => {
        const shadow = el.shadowRoot;
        if (!shadow) return;
        const textarea = shadow.querySelector("textarea");
        if (!textarea) return;

        if (isDark) {
            textarea.style.backgroundColor = "#1e1e1e";
            textarea.style.color = "white";
            textarea.style.border = "none";
            textarea.style.resize = "vertical";
            textarea.style.minHeight = "100px";
            textarea.style.maxHeight = "300px";
            textarea.style.padding = "0.5rem";
            textarea.style.borderRadius = "4px";
            textarea.style.caretColor = "white";
        } else {
            textarea.style.backgroundColor = "";
            textarea.style.color = "";
            textarea.style.border = "";
            textarea.style.resize = "";
            textarea.style.minHeight = "";
            textarea.style.maxHeight = "";
            textarea.style.padding = "";
            textarea.style.borderRadius = "";
            textarea.style.caretColor = "";
        }
    });
}

function applyTheme(isDark) {
    document.body.classList.toggle("dark-mode", isDark);
    document.body.classList.toggle("light-mode", !isDark);
    applyDarkModeToFluentTextAreas(isDark);
    applyCheckBoxStyles(isDark);
}

function applyCheckBoxStyles(isDark) {
    document.querySelectorAll('input[type="checkbox"], .form-check-input').forEach(cb => {
        cb.style.backgroundColor = "transparent";
        cb.style.accentColor = "#0d6efd";
        cb.style.borderColor = "none";
    })
}

// the current preference
let currentDark = window.matchMedia("(prefers-color-scheme: dark)").matches;

// manual override flag
let manualOverride = null;

// apply theme once on load
function detectAndApplyTheme() {
    const isDark = manualOverride !== null ? manualOverride : currentDark;
    applyTheme(isDark);
}

document.addEventListener("DOMContentLoaded", detectAndApplyTheme);

// manual toggle
window.toggleDarkMode = function() {
    const isCurrentlyDark = document.body.classList.contains("dark-mode");
    manualOverride = !isCurrentlyDark;
    applyTheme(manualOverride);
};

// listen for OS changes
window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", e => {
    currentDark = e.matches;
    // clear manual override on system change
    manualOverride = null;
    detectAndApplyTheme();
});

// also re-run on Blazor DOM changes
const observer = new MutationObserver(() => {
    detectAndApplyTheme();
});
observer.observe(document.body, { childList: true, subtree: true });


window.FixDarkModeColors = function () {
    if (document.body.classList.contains("dark-mode")) {
        // Fix ShowDetails text
        document.querySelectorAll(".show-details-text *").forEach(el => {
            const inlineColor = el.style.color;
            if (inlineColor && (
                inlineColor.toLowerCase() === "black" ||
                inlineColor === "#000000" ||
                inlineColor === "#000" ||
                inlineColor === "rgb(0, 0, 0)"
            )) {
                el.style.color = "white";
            }
        });
    }
};

window.FixDarkModeFailureTextArea = function() {
    if (!document.body.classList.contains("dark-mode")) return;

    const textareas = document.querySelectorAll("fluent-text-area.failure-textarea");

    textareas.forEach(fluentTextArea => {
        const shadowRoot = fluentTextArea.shadowRoot;
        if (!shadowRoot) return;

        // The control part is usually a part attribute, so query with ::part
        // But shadowRoot.querySelector('part="control"') is invalid syntax.
        // Instead use shadowRoot.querySelector('[part="control"]')
        const control = shadowRoot.querySelector('[part="control"]');
        if (!control) return;

        control.style.backgroundColor = "#2c2c2c";
        control.style.color = "white";
        control.style.caretColor = "white";
        control.style.borderColor = "#444";
        control.style.padding = "0.5rem";
        control.style.borderRadius = "4px";
        control.style.fontSize = "1rem";
    });
};


