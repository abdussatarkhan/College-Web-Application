/* ==========================================================================
   GDC MADYAN SWAT - OFFICIAL PWA SERVICE WORKER
   Provides offline caching for institutional notices, CSS, logos, and fonts
   ========================================================================== */

const CACHE_NAME = 'gdc-madyan-cache-v1';
const PRECACHE_ASSETS = [
    '/',
    '/Notices',
    '/Admissions/Guide',
    '/css/site.css',
    '/js/site.js',
    '/images/gdc-madyan-logo.png',
    '/images/campus-banner.jpg',
    '/images/principal-rasheed-ahmad.jpg',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/bootstrap/dist/js/bootstrap.bundle.min.js'
];

// 1. Install Event: Cache Core Static Shell
self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            console.log('[ServiceWorker] Pre-caching institutional assets...');
            return cache.addAll(PRECACHE_ASSETS);
        }).then(() => self.skipWaiting())
    );
});

// 2. Activate Event: Clean up legacy caches
self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((name) => {
                    if (name !== CACHE_NAME) {
                        console.log('[ServiceWorker] Purging legacy cache:', name);
                        return caches.delete(name);
                    }
                })
            );
        }).then(() => self.clients.claim())
    );
});

// 3. Fetch Event: Network-first for dynamic content (Notices/Merit), Stale-while-revalidate for static assets
self.addEventListener('fetch', (event) => {
    const request = event.request;
    const url = new URL(request.url);

    // Bypass non-GET requests and internal SignalR/API POSTs
    if (request.method !== 'GET') return;

    // Static Assets (CSS, JS, Fonts, Images): Cache First with Background Refresh
    if (
        url.pathname.startsWith('/css/') ||
        url.pathname.startsWith('/js/') ||
        url.pathname.startsWith('/lib/') ||
        url.pathname.startsWith('/images/') ||
        url.hostname.includes('fonts.gstatic.com') ||
        url.hostname.includes('cdn.jsdelivr.net')
    ) {
        event.respondWith(
            caches.match(request).then((cachedResponse) => {
                if (cachedResponse) {
                    // Refresh in background
                    fetch(request).then((networkResponse) => {
                        if (networkResponse && networkResponse.status === 200) {
                            caches.open(CACHE_NAME).then((cache) => cache.put(request, networkResponse));
                        }
                    }).catch(() => {/* Offline */});
                    return cachedResponse;
                }
                return fetch(request).then((networkResponse) => {
                    if (!networkResponse || networkResponse.status !== 200 || networkResponse.type !== 'basic') {
                        return networkResponse;
                    }
                    const responseToCache = networkResponse.clone();
                    caches.open(CACHE_NAME).then((cache) => cache.put(request, responseToCache));
                    return networkResponse;
                });
            })
        );
        return;
    }

    // HTML Pages (Notices, Home, Guide): Network First, Fallback to Cache
    event.respondWith(
        fetch(request)
            .then((networkResponse) => {
                if (networkResponse && networkResponse.status === 200) {
                    const responseClone = networkResponse.clone();
                    caches.open(CACHE_NAME).then((cache) => cache.put(request, responseClone));
                }
                return networkResponse;
            })
            .catch(async () => {
                const cachedResponse = await caches.match(request);
                if (cachedResponse) {
                    return cachedResponse;
                }
                // If notices was requested offline, try matching /Notices
                if (url.pathname.includes('Notices')) {
                    const noticesCache = await caches.match('/Notices');
                    if (noticesCache) return noticesCache;
                }
                // Fallback to home page
                return caches.match('/');
            })
    );
});
