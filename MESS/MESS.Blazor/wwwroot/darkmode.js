function applyDarkModeToFluentTextAreas(isDark) {
    document.querySelectorAll("fluent-text-area").forEach(el => {
        const shadow = el.shadowRoot;
        if (!shadow) return;

        const existingStyle = shadow.getElementById('theme-mode-injected-style');
        if (existingStyle) existingStyle.remove();

        const style = document.createElement('style');
        style.id = 'theme-mode-injected-style';

        style.textContent = `
            /* Always keep these parts transparent and borderless */
            [part="root"],
            [part="control"],
            [part="field"],
            [role="presentation"],
            *::before,
            *::after {
                background: transparent !important;
                border: none !important;
                box-shadow: none !important;
                outline: none !important;
            }

            /* Style the internal textarea differently based on mode */
            textarea {
                background: ${isDark ? 'transparent' : 'white'} !important;
                color: ${isDark ? 'white' : 'black'} !important;
                caret-color: ${isDark ? 'white' : 'black'} !important;
                border: ${isDark ? 'none' : '1px solid #ccc'} !important;
                border-radius: 4px !important;
                padding: 0.5rem !important;
                resize: vertical !important;
                min-height: 100px !important;
                max-height: 300px !important;
                box-shadow: none !important;
                position: relative !important;
                z-index: 1000 !important;
                transition: background-color 0.3s ease, color 0.3s ease, border-color 0.3s ease !important;
            }
            textarea:focus {
                outline: none !important;
            }
        `;

        shadow.appendChild(style);

        // Reset host styles (optional, can remove if you want)
        el.style.backgroundColor = 'transparent';
        el.style.color = isDark ? 'white' : 'black';

        console.log(`Applied ${isDark ? 'dark' : 'light'} mode with transparent parent and styled textarea`);
    });
}

function applyTheme(isDark) {
    document.body.classList.toggle("dark-mode", isDark);
    document.body.classList.toggle("light-mode", !isDark);
    
    applyDarkModeToFluentTextAreas(isDark);
    applyCheckBoxStyles(isDark);
    
    if (isDark) {
        window.ApplyDarkModeFixToFailureTextAreas();
    }
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



