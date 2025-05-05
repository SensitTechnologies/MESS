const SCROLL_SPEED = 50;
const SCROLL_THRESHOLD = 300;

class DragScroller {
    constructor() {
        this.scrollSpeed = 0;
        this.animationFrame = null;
        this.registerEvents();
    }
    
    registerEvents() {
        document.addEventListener('drag', this.handleDragOver.bind(this));
        document.addEventListener('drop', this.resetScroll.bind(this));
    }

    handleDragOver(e) {
        const isNearTopEdge = e.clientY < SCROLL_THRESHOLD;
        const isNearBottomEdge = (window.innerHeight - e.clientY) < SCROLL_THRESHOLD;
        
        if (isNearTopEdge) {
            this.scrollSpeed = -SCROLL_SPEED;
        } else if (isNearBottomEdge) {
            this.scrollSpeed = SCROLL_SPEED;
        }
        window.scrollBy({
            top: this.scrollSpeed,
            behavior: 'smooth',
        });
    }

    resetScroll() {
        this.scrollSpeed = 0;

        if (this.animationFrame) {
            cancelAnimationFrame(this.animationFrame);
            this.animationFrame = null;
        }
    }
}


function initializeDragScrolling() {
    new DragScroller();
}
