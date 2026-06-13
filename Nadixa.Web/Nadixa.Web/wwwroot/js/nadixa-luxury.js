/* ============================================================
   nadixa-luxury.js
   Elegant & Luxury animation engine for NadixaStore
   Place before </body> in _Layout.cshtml:
   <script src="~/js/nadixa-luxury.js"></script>
   ============================================================ */

(function () {
    'use strict';

    /* ── 1. SCROLL REVEAL ────────────────────────────────────── */
    function initScrollReveal() {
        var singles = document.querySelectorAll('.nx-reveal');
        var groups = document.querySelectorAll('.nx-reveal-group');

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('nx-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });

        singles.forEach(function (el) { observer.observe(el); });
        groups.forEach(function (el) { observer.observe(el); });

        /* Also add nx-reveal to common sections automatically */
        var autoReveal = document.querySelectorAll(
            '.block2, .p-b-63, .how5-content, .sec-head, ' +
            '.flex-w.flex-t.p-t-14, .dash-card, .metric-card'
        );
        autoReveal.forEach(function (el, i) {
            if (!el.classList.contains('nx-reveal')) {
                el.classList.add('nx-reveal');
                el.style.transitionDelay = (i % 4) * 0.08 + 's';
                observer.observe(el);
            }
        });
    }

    /* ── 2. SECTION TITLE LINE ───────────────────────────────── */
    function initTitleLines() {
        var titles = document.querySelectorAll('.ltext-106, .ltext-109, .mtext-111, .ltext-105');
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('nx-title', 'nx-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.4 });
        titles.forEach(function (el) { observer.observe(el); });
    }

    /* ── 3. BACK TO TOP ──────────────────────────────────────── */
    function initBackToTop() {
        var btn = document.createElement('button');
        btn.id = 'nx-back-top';
        btn.setAttribute('aria-label', 'Back to top');
        btn.innerHTML = '&#8679;';
        document.body.appendChild(btn);

        window.addEventListener('scroll', function () {
            if (window.scrollY > 320) {
                btn.classList.add('nx-show');
            } else {
                btn.classList.remove('nx-show');
            }
        }, { passive: true });

        btn.addEventListener('click', function () {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }

    /* ── 4. HEADER SHADOW ON SCROLL ─────────────────────────── */
    function initHeaderScroll() {
        var header = document.querySelector('header');
        if (!header) return;
        window.addEventListener('scroll', function () {
            if (window.scrollY > 10) {
                header.classList.add('nx-scrolled');
            } else {
                header.classList.remove('nx-scrolled');
            }
        }, { passive: true });
    }

    /* ── 5. IMAGE FADE-IN ON LOAD ────────────────────────────── */
    function initImageFade() {
        document.querySelectorAll('img').forEach(function (img) {
            if (!img.complete) {
                img.classList.add('nx-img-loading');
                img.addEventListener('load', function () {
                    img.classList.remove('nx-img-loading');
                    img.classList.add('nx-img-loaded');
                });
            }
        });
    }

    /* ── 6. RIPPLE EFFECT ON PRIMARY BUTTONS ─────────────────── */
    function initRipple() {
        document.querySelectorAll(
            '.flex-c-m.bg3, .flex-c-m.bg1, .btn-primary, ' +
            '.hov-btn1, .hov-btn3, .add-cart-btn, .btn-primary'
        ).forEach(function (btn) {
            btn.classList.add('nx-ripple');
            btn.addEventListener('click', function (e) {
                var rect = btn.getBoundingClientRect();
                var size = Math.max(rect.width, rect.height);
                var circle = document.createElement('span');
                circle.className = 'nx-ripple-circle';
                circle.style.cssText =
                    'width:' + size + 'px;height:' + size + 'px;' +
                    'left:' + (e.clientX - rect.left - size / 2) + 'px;' +
                    'top:' + (e.clientY - rect.top - size / 2) + 'px;';
                btn.appendChild(circle);
                setTimeout(function () { circle.remove(); }, 560);
            });
        });
    }

    /* ── 7. WISHLIST PULSE ───────────────────────────────────── */
    function initWishlistPulse() {
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.js-addwish-detail, .btn-addwish-b2');
            if (!btn) return;
            btn.classList.remove('nx-pulse');
            void btn.offsetWidth; /* reflow to restart */
            btn.classList.add('nx-pulse');
            setTimeout(function () { btn.classList.remove('nx-pulse'); }, 450);
        });
    }

    /* ── 8. ADD TO CART ANIMATION ────────────────────────────── */
    function initCartAnimation() {
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.js-addcart-detail, .add-cart-btn');
            if (!btn) return;
            btn.classList.remove('nx-cart-added');
            void btn.offsetWidth;
            btn.classList.add('nx-cart-added');
            setTimeout(function () { btn.classList.remove('nx-cart-added'); }, 510);
        });
    }

    /* ── 9. STAGGER PRODUCT GRID ON PAGE LOAD ────────────────── */
    function initGridStagger() {
        var cards = document.querySelectorAll('.isotope-item, .block2');
        cards.forEach(function (card, i) {
            card.style.opacity = '0';
            card.style.transform = 'translateY(24px)';
            card.style.transition = 'opacity .55s ease, transform .55s ease';
            setTimeout(function () {
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 120 + i * 75);
        });
    }

    /* ── 10. FILTER BUTTON SMOOTH REFLOW ─────────────────────── */
    function initFilterSmooth() {
        document.querySelectorAll('.how-filter-btn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                document.querySelectorAll('.isotope-item').forEach(function (item) {
                    item.style.transition = 'opacity .4s ease, transform .4s ease';
                });
            });
        });
    }

    /* ── INIT ALL ─────────────────────────────────────────────── */
    function init() {
        initScrollReveal();
        initTitleLines();
        initBackToTop();
        initHeaderScroll();
        initImageFade();
        initRipple();
        initWishlistPulse();
        initCartAnimation();
        initGridStagger();
        initFilterSmooth();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

}());
