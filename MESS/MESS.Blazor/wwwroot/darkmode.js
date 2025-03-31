document.addEventListener('DOMContentLoaded', () => {
    const isDarkMode = localStorage.getItem('darkMode') === 'true';
    toggleDarkMode(isDarkMode);
});

function toggleDarkMode(isDarkMode) {
    document.body.className = isDarkMode ? 'dark-mode' : 'light-mode';
    localStorage.setItem('darkMode', isDarkMode);
}
