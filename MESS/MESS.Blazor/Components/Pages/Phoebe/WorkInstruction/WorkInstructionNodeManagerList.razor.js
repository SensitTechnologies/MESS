const SCROLL_SPEED = 30;
const SCROLL_THRESHOLD = 350;

class DragScroller {
    constructor() {
        this.scrollSpeed = 0;
        this.animationFrame = null;
        this.registerEvents();
    }
    
    registerEvents() {
        document.addEventListener('dragover', this.handleDragOver.bind(this));
        document.addEventListener('dragleave', this.resetScroll.bind(this));
        document.addEventListener('drop', this.resetScroll.bind(this));
    }

    smoothScroll(){
        if (this.scrollSpeed !== 0) {
            window.scrollBy(0, this.scrollSpeed);
            this.animationFrame = requestAnimationFrame(this.smoothScroll);
        }
    }

    handleDragOver(e) {
        const isNearTopEdge = e.clientY < SCROLL_THRESHOLD;
        const isNearBottomEdge = (window.innerHeight - e.clientY) < SCROLL_THRESHOLD;

        if (isNearTopEdge) {
            this.scrollSpeed = -SCROLL_SPEED;
        } else if (isNearBottomEdge) {
            this.scrollSpeed = SCROLL_SPEED;
        } else {
            this.scrollSpeed = 0;
        }

        if (!this.animationFrame) {
            this.animationFrame = requestAnimationFrame(this.smoothScroll.bind(this));
        }
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
