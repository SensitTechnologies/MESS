export function initCarousel(id, interval) {
    // Bootstrap's carousel API
    const carousel = new bootstrap.Carousel(document.getElementById(id), {
        interval: interval,
        wrap: true
    });

    return {
        next: () => carousel.next(),
        prev: () => carousel.prev(),
        pause: () => carousel.pause(),
        cycle: () => carousel.cycle()
    };
}