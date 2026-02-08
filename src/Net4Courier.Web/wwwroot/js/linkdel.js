window.linkdel = {
  _deferredPrompt: null,

  registerServiceWorker: async function () {
    if ('serviceWorker' in navigator) {
      try {
        const reg = await navigator.serviceWorker.register('/linkdel/sw.js', { scope: '/linkdel/' });
        console.log('[LinkDel] SW registered:', reg.scope);
      } catch (e) {
        console.warn('[LinkDel] SW registration failed:', e);
      }
    }

    window.addEventListener('beforeinstallprompt', (e) => {
      e.preventDefault();
      window.linkdel._deferredPrompt = e;
    });
  },

  promptInstall: async function () {
    const prompt = window.linkdel._deferredPrompt;
    if (!prompt) return false;
    prompt.prompt();
    const result = await prompt.userChoice;
    window.linkdel._deferredPrompt = null;
    return result.outcome === 'accepted';
  },

  getCurrentPosition: function () {
    return new Promise((resolve, reject) => {
      if (!navigator.geolocation) {
        reject('Geolocation not supported');
        return;
      }
      navigator.geolocation.getCurrentPosition(
        (pos) => {
          resolve({ lat: pos.coords.latitude, lng: pos.coords.longitude, accuracy: pos.coords.accuracy });
        },
        (err) => reject(err.message),
        { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 }
      );
    });
  },

  openCamera: function (elementId) {
    const el = document.getElementById(elementId);
    if (el) el.click();
  },

  captureBarcode: function () {
    return new Promise((resolve) => {
      resolve(null);
    });
  },

  openNavigation: function (lat, lng, address) {
    const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent);
    let url;
    if (lat && lng) {
      url = isIOS
        ? 'maps://maps.apple.com/?daddr=' + lat + ',' + lng
        : 'https://www.google.com/maps/dir/?api=1&destination=' + lat + ',' + lng;
    } else if (address) {
      url = isIOS
        ? 'maps://maps.apple.com/?daddr=' + encodeURIComponent(address)
        : 'https://www.google.com/maps/dir/?api=1&destination=' + encodeURIComponent(address);
    }
    if (url) window.open(url, '_blank');
  },

  vibrate: function (pattern) {
    if (navigator.vibrate) {
      navigator.vibrate(pattern || [200]);
    }
  },

  showNotification: async function (title, body) {
    if (!('Notification' in window)) return;
    if (Notification.permission === 'default') {
      await Notification.requestPermission();
    }
    if (Notification.permission === 'granted') {
      new Notification(title, { body: body, icon: '/linkdel/icon-192.png' });
    }
  },

  requestNotificationPermission: async function () {
    if (!('Notification' in window)) {
      console.warn('[LinkDel] Notifications not supported');
      return 'unsupported';
    }
    if (Notification.permission === 'granted') {
      return 'granted';
    }
    const result = await Notification.requestPermission();
    return result;
  },

  getNotificationPermission: function () {
    if (!('Notification' in window)) return 'unsupported';
    return Notification.permission;
  },

  subscribePush: async function (vapidPublicKey) {
    if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
      return null;
    }
    try {
      const reg = await navigator.serviceWorker.ready;
      let sub = await reg.pushManager.getSubscription();
      if (sub) return JSON.stringify(sub.toJSON());

      if (!vapidPublicKey) {
        console.warn('[LinkDel] No VAPID key provided, cannot create push subscription');
        return null;
      }

      const applicationServerKey = window.linkdel._urlBase64ToUint8Array(vapidPublicKey);
      sub = await reg.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: applicationServerKey
      });
      console.log('[LinkDel] Push subscription created');
      return JSON.stringify(sub.toJSON());
    } catch (e) {
      console.warn('[LinkDel] Push subscribe error:', e);
      return null;
    }
  },

  _urlBase64ToUint8Array: function (base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding).replace(/\-/g, '+').replace(/_/g, '/');
    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);
    for (let i = 0; i < rawData.length; ++i) {
      outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
  }
};
