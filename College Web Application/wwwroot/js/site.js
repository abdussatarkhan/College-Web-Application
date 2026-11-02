/**
 * Government Degree College Madyan Swat - Core Institutional Client Scripts
 * Features: Dark Mode, English/Urdu RTL Toggle, Animated Stat Counters, PWA Service Worker, Scroll-Reveal
 */

(function () {
    'use strict';

    // 1. Initialize PWA Service Worker
    if ('serviceWorker' in navigator) {
        window.addEventListener('load', () => {
            navigator.serviceWorker.register('/sw.js')
                .then(reg => console.log('[GDC PWA] Service Worker registered with scope:', reg.scope))
                .catch(err => console.warn('[GDC PWA] Service Worker registration failed:', err));
        });
    }

    // 2. Theme Toggle (Light / Dark Mode)
    const THEME_KEY = 'gdc_theme';
    const currentTheme = localStorage.getItem(THEME_KEY) || (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
    document.documentElement.setAttribute('data-theme', currentTheme);

    window.toggleTheme = function () {
        const activeTheme = document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', activeTheme);
        localStorage.setItem(THEME_KEY, activeTheme);
        updateThemeIcons(activeTheme);
    };

    function updateThemeIcons(theme) {
        const icons = document.querySelectorAll('.theme-toggle-icon');
        icons.forEach(icon => {
            if (theme === 'dark') {
                icon.className = 'bi bi-sun-fill theme-toggle-icon text-warning';
            } else {
                icon.className = 'bi bi-moon-stars-fill theme-toggle-icon text-white';
            }
        });
    }

    // 3. Language & RTL Toggle (English <-> Urdu)
    const LANG_KEY = 'gdc_language';
    const activeLang = localStorage.getItem(LANG_KEY) || 'en';
    applyLanguage(activeLang);

    window.toggleLanguage = function () {
        const newLang = (document.documentElement.getAttribute('lang') === 'ur') ? 'en' : 'ur';
        applyLanguage(newLang);
        localStorage.setItem(LANG_KEY, newLang);
    };

    function applyLanguage(lang) {
        document.documentElement.setAttribute('lang', lang);
        if (lang === 'ur') {
            document.documentElement.setAttribute('dir', 'rtl');
            document.body.classList.add('urdu-font');
            const langLabels = document.querySelectorAll('.lang-toggle-label');
            langLabels.forEach(el => el.textContent = 'English');
        } else {
            document.documentElement.setAttribute('dir', 'ltr');
            document.body.classList.remove('urdu-font');
            const langLabels = document.querySelectorAll('.lang-toggle-label');
            langLabels.forEach(el => el.textContent = 'اردو');
        }
    }

    // 4. Animated Stat Number Counters
    function initStatCounters() {
        const counters = document.querySelectorAll('[data-counter-target]');
        if (!counters.length) return;

        const observer = new IntersectionObserver((entries, obs) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const el = entry.target;
                    const target = parseInt(el.getAttribute('data-counter-target'), 10) || 0;
                    const suffix = el.getAttribute('data-counter-suffix') || '';
                    const duration = 2000;
                    const startTime = performance.now();

                    function updateCounter(currentTime) {
                        const elapsed = currentTime - startTime;
                        const progress = Math.min(elapsed / duration, 1);
                        // Ease-out cubic formula
                        const easeOut = 1 - Math.pow(1 - progress, 3);
                        const currentVal = Math.floor(easeOut * target);

                        el.textContent = currentVal.toLocaleString() + suffix;

                        if (progress < 1) {
                            requestAnimationFrame(updateCounter);
                        } else {
                            el.textContent = target.toLocaleString() + suffix;
                        }
                    }

                    requestAnimationFrame(updateCounter);
                    obs.unobserve(el);
                }
            });
        }, { threshold: 0.3 });

        counters.forEach(counter => observer.observe(counter));
    }

    // 5. Scroll-Reveal Animation Trigger
    function initScrollReveal() {
        const elements = document.querySelectorAll('.reveal-fade-up');
        if (!elements.length) return;

        const observer = new IntersectionObserver((entries, obs) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('revealed');
                    obs.unobserve(entry.target);
                }
            });
        }, { threshold: 0.15 });

        elements.forEach(el => observer.observe(el));
    }

    // 6. Mobile & Data-Saver Video Optimizer
    function initVideoOptimizer() {
        const video = document.querySelector('.hero-video-bg');
        if (!video) return;

        // If user is on a mobile device or cellular connection with Save-Data
        if (window.innerWidth < 768 || (navigator.connection && navigator.connection.saveData)) {
            video.pause();
            video.removeAttribute('autoplay');
            video.style.display = 'none';
        }
    }

    // Run on DOM Ready
    document.addEventListener('DOMContentLoaded', () => {
        updateThemeIcons(currentTheme);
        initStatCounters();
        initScrollReveal();
        initVideoOptimizer();
    });
})();
