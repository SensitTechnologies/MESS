function initializeDragScrolling() {
    let scrollSpeed = 0;
    let animationFrame;

    function smoothScroll() {
        if (scrollSpeed !== 0) {
            window.scrollBy(0, scrollSpeed);
            animationFrame = requestAnimationFrame(smoothScroll);
        }
    }

    document.addEventListener('dragover', (e) => {
        const scrollThreshold = 200;
        const topEdge = e.clientY < scrollThreshold;
        const bottomEdge = (window.innerHeight - e.clientY) < scrollThreshold;

        if (topEdge) {
            scrollSpeed = -10;
        } else if (bottomEdge) {
            scrollSpeed = 10;
        } else {
            scrollSpeed = 0;
        }

        if (!animationFrame) {
            animationFrame = requestAnimationFrame(smoothScroll);
        }
    });

    document.addEventListener('dragleave', () => {
        scrollSpeed = 0;
        cancelAnimationFrame(animationFrame);
        animationFrame = null;
    })
    
    document.addEventListener('drop', () => {
        scrollSpeed = 0;
        cancelAnimationFrame(animationFrame);
        animationFrame = null;
    })
    
    // Allow for user to scroll whilst dragging
    document.addEventListener('wheel', (e) => {
        window.scrollBy(0, e.deltaY);
    }, { passive: true });
}
