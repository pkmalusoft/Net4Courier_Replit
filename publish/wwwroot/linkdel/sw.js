const CACHE_VERSION = 'linkdel-v2';
const CACHE_ASSETS = [
  '/linkdel/manifest.json',
  '/linkdel/icon-192.png',
  '/linkdel/icon-512.png'
];

const OFFLINE_PAGE = '/linkdel/offline';

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_VERSION).then((cache) => {
      return cache.addAll(CACHE_ASSETS);
    })
  );
  self.skipWaiting();
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) => {
      return Promise.all(
        keys.filter((key) => key !== CACHE_VERSION).map((key) => caches.delete(key))
      );
    })
  );
  self.clients.claim();
});

self.addEventListener('fetch', (event) => {
  const url = new URL(event.request.url);

  if (url.pathname.startsWith('/_blazor') || url.pathname.startsWith('/_framework')) {
    event.respondWith(fetch(event.request));
    return;
  }

  event.respondWith(
    fetch(event.request)
      .then((response) => {
        if (response.ok && event.request.method === 'GET') {
          const clone = response.clone();
          caches.open(CACHE_VERSION).then((cache) => {
            cache.put(event.request, clone);
          });
        }
        return response;
      })
      .catch(() => {
        return caches.match(event.request).then((cached) => {
          if (cached) return cached;
          if (event.request.mode === 'navigate') {
            return new Response(
              '<!DOCTYPE html><html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>LinkDel - Offline</title><style>body{font-family:sans-serif;display:flex;align-items:center;justify-content:center;min-height:100vh;margin:0;background:#1e293b;color:#fff;text-align:center}.c{padding:2rem}h1{font-size:1.5rem;margin-bottom:.5rem}p{color:#94a3b8;margin-bottom:1.5rem}button{background:#556ee6;color:#fff;border:none;padding:.75rem 2rem;border-radius:8px;font-size:1rem;cursor:pointer}</style></head><body><div class="c"><h1>You\'re Offline</h1><p>Check your internet connection and try again.</p><button onclick="location.reload()">Retry</button></div></body></html>',
              { headers: { 'Content-Type': 'text/html' } }
            );
          }
          return new Response('', { status: 503 });
        });
      })
  );
});

self.addEventListener('push', (event) => {
  let data = { title: 'LinkDel', body: 'You have a new notification' };
  try {
    if (event.data) {
      data = event.data.json();
    }
  } catch (e) {
    if (event.data) {
      data.body = event.data.text();
    }
  }

  event.waitUntil(
    self.registration.showNotification(data.title || 'LinkDel', {
      body: data.body || '',
      icon: '/linkdel/icon-192.png',
      badge: '/linkdel/icon-192.png',
      data: data.url || '/linkdel',
      vibrate: [200, 100, 200]
    })
  );
});

self.addEventListener('notificationclick', (event) => {
  event.notification.close();
  const url = event.notification.data || '/linkdel';
  event.waitUntil(
    clients.matchAll({ type: 'window' }).then((windowClients) => {
      for (const client of windowClients) {
        if (client.url.includes('/linkdel') && 'focus' in client) {
          return client.focus();
        }
      }
      if (clients.openWindow) {
        return clients.openWindow(url);
      }
    })
  );
});

self.addEventListener('sync', (event) => {
  if (event.tag === 'linkdel-sync') {
    event.waitUntil(syncOfflineQueue());
  }
});

async function syncOfflineQueue() {
  try {
    const allClients = await clients.matchAll({ type: 'window' });
    for (const client of allClients) {
      client.postMessage({ type: 'SYNC_REQUESTED' });
    }
  } catch (e) {
    console.warn('[LinkDel SW] Sync error:', e);
  }
}
