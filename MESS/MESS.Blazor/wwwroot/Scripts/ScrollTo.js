export function scrollTo(elementId) {
    const el = document.getElementById(elementId);
    if (el) {
        el?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
}

export function ScrollToTop() {
    window.scrollTo({top: 0, left: 0, behavior: 'smooth'});
}