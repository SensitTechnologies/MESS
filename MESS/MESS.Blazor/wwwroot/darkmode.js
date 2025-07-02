function getSharedTextAreaStyles(isDark) {
    return `
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

        textarea {
            background: ${isDark ? '#2c2c2c' : '#fff'} !important;
            color: ${isDark ? '#fff' : '#000'} !important;
            caret-color: ${isDark ? '#fff' : '#000'} !important;
            border: ${isDark ? '1px solid #555' : '1px solid #ccc'} !important;
            border-radius: 4px !important;
            padding: 0.5rem !important;
            resize: vertical !important;
            min-height: 100px !important;
            max-height: 300px !important;
            box-shadow: none !important;
            position: relative !important;
            z-index: 1000 !important;
            font-size: 1rem !important;
            transition: background-color 0.3s ease, color 0.3s ease, border-color 0.3s ease !important;
        }

        textarea:focus {
            outline: none !important;
        }
    `;
}

function applyDarkModeToFluentTextAreas(isDark) {
    document.querySelectorAll("fluent-text-area").forEach(el => {
        const shadow = el.shadowRoot;
        if (!shadow) return;

        const existingStyle = shadow.getElementById('theme-mode-injected-style');
        if (existingStyle) existingStyle.remove();

        const style = document.createElement('style');
        style.id = 'theme-mode-injected-style';
        style.textContent = getSharedTextAreaStyles(isDark);
        shadow.appendChild(style);

        el.style.backgroundColor = 'transparent';
        el.style.color = isDark ? '#fff' : '#000';
    });
}

function applyTheme(isDark) {
    document.body.classList.toggle("dark-mode", isDark);
    document.body.classList.toggle("light-mode", !isDark);
    applyDarkModeToFluentTextAreas(isDark);

    if (isDark && typeof window.ApplyDarkModeFixToFailureTextAreas === "function") {
        window.ApplyDarkModeFixToFailureTextAreas();
    }
}

let currentDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
let manualOverride = null;

function detectAndApplyTheme() {
    const isDark = manualOverride !== null ? manualOverride : currentDark;
    applyTheme(isDark);
}

document.addEventListener("DOMContentLoaded", detectAndApplyTheme);

window.toggleDarkMode = function () {
    const isCurrentlyDark = document.body.classList.contains("dark-mode");
    manualOverride = !isCurrentlyDark;
    applyTheme(manualOverride);
};

window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", e => {
    currentDark = e.matches;
    manualOverride = null;
    detectAndApplyTheme();
});

const observer = new MutationObserver(() => {
    detectAndApplyTheme();
});
observer.observe(document.body, { childList: true, subtree: true });

window.FixDarkModeColors = function () {
    if (document.body.classList.contains("dark-mode")) {
        document.querySelectorAll(".show-details-text *").forEach(el => {
            const inlineColor = el.style.color;
            if (inlineColor && ["black", "#000000", "#000", "rgb(0, 0, 0)"].includes(inlineColor.toLowerCase())) {
                el.style.color = "white";
            }
        });
    }
};

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