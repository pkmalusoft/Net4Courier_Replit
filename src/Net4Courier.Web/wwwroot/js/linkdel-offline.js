window.linkdelOffline = {
  DB_NAME: 'LinkDelDB',
  DB_VERSION: 1,
  _db: null,

  openDB: function () {
    return new Promise((resolve, reject) => {
      if (window.linkdelOffline._db) {
        resolve(window.linkdelOffline._db);
        return;
      }

      const request = indexedDB.open(window.linkdelOffline.DB_NAME, window.linkdelOffline.DB_VERSION);

      request.onupgradeneeded = (event) => {
        const db = event.target.result;

        if (!db.objectStoreNames.contains('pickups')) {
          const pickupStore = db.createObjectStore('pickups', { keyPath: 'id' });
          pickupStore.createIndex('status', 'status', { unique: false });
        }

        if (!db.objectStoreNames.contains('deliveries')) {
          const deliveryStore = db.createObjectStore('deliveries', { keyPath: 'id' });
          deliveryStore.createIndex('status', 'status', { unique: false });
        }

        if (!db.objectStoreNames.contains('expenses')) {
          const expenseStore = db.createObjectStore('expenses', { keyPath: 'id' });
          expenseStore.createIndex('date', 'expenseDate', { unique: false });
        }

        if (!db.objectStoreNames.contains('outbox')) {
          const outboxStore = db.createObjectStore('outbox', { keyPath: 'id', autoIncrement: true });
          outboxStore.createIndex('status', 'status', { unique: false });
          outboxStore.createIndex('createdAt', 'createdAt', { unique: false });
        }

        if (!db.objectStoreNames.contains('meta')) {
          db.createObjectStore('meta', { keyPath: 'key' });
        }
      };

      request.onsuccess = (event) => {
        window.linkdelOffline._db = event.target.result;
        resolve(event.target.result);
      };

      request.onerror = (event) => {
        console.error('[LinkDel Offline] DB open error:', event.target.error);
        reject(event.target.error);
      };
    });
  },

  cacheData: async function (storeName, items) {
    try {
      const db = await window.linkdelOffline.openDB();
      const tx = db.transaction(storeName, 'readwrite');
      const store = tx.objectStore(storeName);

      for (const item of items) {
        store.put(item);
      }

      await new Promise((resolve, reject) => {
        tx.oncomplete = resolve;
        tx.onerror = () => reject(tx.error);
      });

      await window.linkdelOffline._setMeta(storeName + '_lastSync', new Date().toISOString());
      return true;
    } catch (e) {
      console.error('[LinkDel Offline] Cache error:', e);
      return false;
    }
  },

  getCachedData: async function (storeName) {
    try {
      const db = await window.linkdelOffline.openDB();
      const tx = db.transaction(storeName, 'readonly');
      const store = tx.objectStore(storeName);

      return new Promise((resolve, reject) => {
        const request = store.getAll();
        request.onsuccess = () => resolve(request.result || []);
        request.onerror = () => reject(request.error);
      });
    } catch (e) {
      console.error('[LinkDel Offline] Get cached error:', e);
      return [];
    }
  },

  clearStore: async function (storeName) {
    try {
      const db = await window.linkdelOffline.openDB();
      const tx = db.transaction(storeName, 'readwrite');
      const store = tx.objectStore(storeName);
      store.clear();
      await new Promise((resolve, reject) => {
        tx.oncomplete = resolve;
        tx.onerror = () => reject(tx.error);
      });
      return true;
    } catch (e) {
      console.error('[LinkDel Offline] Clear error:', e);
      return false;
    }
  },

  enqueueAction: async function (actionType, payload) {
    try {
      const db = await window.linkdelOffline.openDB();
      const tx = db.transaction('outbox', 'readwrite');
      const store = tx.objectStore('outbox');

      store.add({
        actionType: actionType,
        payload: payload,
        status: 'pending',
        createdAt: new Date().toISOString(),
        retryCount: 0
      });

      await new Promise((resolve, reject) => {
        tx.oncomplete = resolve;
        tx.onerror = () => reject(tx.error);
      });

      if ('serviceWorker' in navigator && 'SyncManager' in window) {
        const reg = await navigator.serviceWorker.ready;
        await reg.sync.register('linkdel-sync');
      }

      return true;
    } catch (e) {
      console.error('[LinkDel Offline] Enqueue error:', e);
      return false;
    }
  },

  getPendingActions: async function () {
    try {
      const db = await window.linkdelOffline.openDB();
      const tx = db.transaction('outbox', 'readonly');
      const store = tx.objectStore('outbox');
      const index = store.index('status');

      return new Promise((resolve, reject) => {
        const request = index.getAll('pending');
        request.onsuccess = () => resolve(request.result || []);
        request.onerror = () => reject(request.error);
      });
    } catch (e) {
      console.error('[LinkDel Offline] Get pending error:', e);
      return [];
    }
  },

  markActionSynced: async function (id) {
    try {
      const db = await window.linkdelOffline.openDB();
      const tx = db.transaction('outbox', 'readwrite');
      const store = tx.objectStore('outbox');

      return new Promise((resolve, reject) => {
        const getReq = store.get(id);
        getReq.onsuccess = () => {
          const item = getReq.result;
          if (item) {
            item.status = 'synced';
            item.syncedAt = new Date().toISOString();
            store.put(item);
          }
          tx.oncomplete = () => resolve(true);
          tx.onerror = () => reject(tx.error);
        };
        getReq.onerror = () => reject(getReq.error);
      });
    } catch (e) {
      console.error('[LinkDel Offline] Mark synced error:', e);
      return false;
    }
  },

  markActionFailed: async function (id) {
    try {
      const db = await window.linkdelOffline.openDB();
      const tx = db.transaction('outbox', 'readwrite');
      const store = tx.objectStore('outbox');

      return new Promise((resolve, reject) => {
        const getReq = store.get(id);
        getReq.onsuccess = () => {
          const item = getReq.result;
          if (item) {
            item.retryCount = (item.retryCount || 0) + 1;
            if (item.retryCount >= 5) {
              item.status = 'failed';
            }
            store.put(item);
          }
          tx.oncomplete = () => resolve(true);
          tx.onerror = () => reject(tx.error);
        };
        getReq.onerror = () => reject(getReq.error);
      });
    } catch (e) {
      console.error('[LinkDel Offline] Mark failed error:', e);
      return false;
    }
  },

  clearSyncedActions: async function () {
    try {
      const db = await window.linkdelOffline.openDB();
      const tx = db.transaction('outbox', 'readwrite');
      const store = tx.objectStore('outbox');
      const index = store.index('status');

      return new Promise((resolve, reject) => {
        const request = index.openCursor('synced');
        request.onsuccess = (event) => {
          const cursor = event.target.result;
          if (cursor) {
            cursor.delete();
            cursor.continue();
          }
        };
        tx.oncomplete = () => resolve(true);
        tx.onerror = () => reject(tx.error);
      });
    } catch (e) {
      console.error('[LinkDel Offline] Clear synced error:', e);
      return false;
    }
  },

  getOutboxCount: async function () {
    try {
      const db = await window.linkdelOffline.openDB();
      const tx = db.transaction('outbox', 'readonly');
      const store = tx.objectStore('outbox');
      const index = store.index('status');

      return new Promise((resolve, reject) => {
        const request = index.count('pending');
        request.onsuccess = () => resolve(request.result || 0);
        request.onerror = () => reject(request.error);
      });
    } catch (e) {
      return 0;
    }
  },

  getLastSync: async function (storeName) {
    try {
      return await window.linkdelOffline._getMeta(storeName + '_lastSync');
    } catch (e) {
      return null;
    }
  },

  isOnline: function () {
    return navigator.onLine;
  },

  _listeners: [],

  registerConnectivityListeners: function (dotNetRef) {
    window.linkdelOffline.removeConnectivityListeners();

    const onOnline = () => dotNetRef.invokeMethodAsync('OnConnectivityChanged', true);
    const onOffline = () => dotNetRef.invokeMethodAsync('OnConnectivityChanged', false);
    const onMessage = (event) => {
      if (event.data && event.data.type === 'SYNC_REQUESTED') {
        dotNetRef.invokeMethodAsync('OnSyncRequested');
      }
    };

    window.addEventListener('online', onOnline);
    window.addEventListener('offline', onOffline);
    window.linkdelOffline._listeners.push({ type: 'online', fn: onOnline });
    window.linkdelOffline._listeners.push({ type: 'offline', fn: onOffline });

    if ('serviceWorker' in navigator) {
      navigator.serviceWorker.addEventListener('message', onMessage);
      window.linkdelOffline._listeners.push({ type: 'sw-message', fn: onMessage });
    }
  },

  removeConnectivityListeners: function () {
    for (const l of window.linkdelOffline._listeners) {
      if (l.type === 'sw-message') {
        if ('serviceWorker' in navigator) {
          navigator.serviceWorker.removeEventListener('message', l.fn);
        }
      } else {
        window.removeEventListener(l.type, l.fn);
      }
    }
    window.linkdelOffline._listeners = [];
  },

  _setMeta: async function (key, value) {
    const db = await window.linkdelOffline.openDB();
    const tx = db.transaction('meta', 'readwrite');
    const store = tx.objectStore('meta');
    store.put({ key: key, value: value });
    await new Promise((resolve, reject) => {
      tx.oncomplete = resolve;
      tx.onerror = () => reject(tx.error);
    });
  },

  _getMeta: async function (key) {
    const db = await window.linkdelOffline.openDB();
    const tx = db.transaction('meta', 'readonly');
    const store = tx.objectStore('meta');
    return new Promise((resolve, reject) => {
      const request = store.get(key);
      request.onsuccess = () => resolve(request.result ? request.result.value : null);
      request.onerror = () => reject(request.error);
    });
  }
};
