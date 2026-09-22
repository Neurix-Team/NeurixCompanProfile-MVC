document.addEventListener('DOMContentLoaded', () => {
    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const cards = document.querySelectorAll('main .magnetic-card');
    cards.forEach(card => card.classList.remove('opacity-0', 'translate-y-6'));

    if (reduced || !('IntersectionObserver' in window)) return;

    const reveal = new IntersectionObserver(entries => {
        let stagger = 0;
        entries.forEach(entry => {
            if (!entry.isIntersecting) return;
            const element = entry.target;
            reveal.unobserve(element);
            element.classList.remove('neurix-reveal-pending');
            const card = element.classList.contains('magnetic-card');
            element.animate(card ? [
                { opacity: 0, transform: 'perspective(1200px) translateY(80px) rotateX(-50deg) scale(0.88)', transformOrigin: 'center bottom' },
                { opacity: 1, transform: 'perspective(1200px) translateY(-4px) rotateX(3deg) scale(1.015)', offset: 0.8, transformOrigin: 'center bottom' },
                { opacity: 1, transform: 'perspective(1200px) translateY(0) rotateX(0deg) scale(1)', transformOrigin: 'center bottom' }
            ] : [
                { opacity: 0.3, transform: 'translateY(24px) scale(0.98)' },
                { opacity: 1, transform: 'translateY(0) scale(1)' }
            ], { duration: card ? 720 : 650, delay: card ? Math.min(stagger++ * 75, 225) : 0, fill: 'backwards', easing: 'cubic-bezier(0.22, 1, 0.36, 1)' });
        });
    }, { threshold: 0.08 });

    const images = document.querySelectorAll('main img.object-cover, main img.object-contain');
    images.forEach(img => {
        if (img.closest('[aria-hidden="true"], .absolute.inset-0')) return;
        img.classList.add('neurix-editorial-image');
        if (!img.closest('.magnetic-card')) reveal.observe(img);
    });
    cards.forEach(card => {
        card.classList.add('neurix-reveal-pending');
        card.addEventListener('focusin', () => {
            card.classList.remove('neurix-reveal-pending');
            reveal.unobserve(card);
        }, { once: true });
        reveal.observe(card);
    });
    window.addEventListener('pageshow', event => {
        if (event.persisted) cards.forEach(card => card.classList.remove('neurix-reveal-pending'));
    });
});
