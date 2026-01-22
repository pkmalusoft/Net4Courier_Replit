let signatureCanvas = null;
let signatureCtx = null;
let isDrawing = false;
let lastX = 0;
let lastY = 0;

window.initSignatureCanvas = function(canvasId) {
    signatureCanvas = document.getElementById(canvasId);
    if (!signatureCanvas) return;
    
    signatureCtx = signatureCanvas.getContext('2d');
    signatureCtx.strokeStyle = '#000';
    signatureCtx.lineWidth = 2;
    signatureCtx.lineCap = 'round';
    signatureCtx.lineJoin = 'round';
    
    signatureCanvas.addEventListener('mousedown', startDrawing);
    signatureCanvas.addEventListener('mousemove', draw);
    signatureCanvas.addEventListener('mouseup', stopDrawing);
    signatureCanvas.addEventListener('mouseout', stopDrawing);
    
    signatureCanvas.addEventListener('touchstart', handleTouchStart, { passive: false });
    signatureCanvas.addEventListener('touchmove', handleTouchMove, { passive: false });
    signatureCanvas.addEventListener('touchend', stopDrawing);
    
    clearCanvas(canvasId);
};

function startDrawing(e) {
    isDrawing = true;
    const rect = signatureCanvas.getBoundingClientRect();
    lastX = e.clientX - rect.left;
    lastY = e.clientY - rect.top;
}

function draw(e) {
    if (!isDrawing) return;
    
    const rect = signatureCanvas.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    
    signatureCtx.beginPath();
    signatureCtx.moveTo(lastX, lastY);
    signatureCtx.lineTo(x, y);
    signatureCtx.stroke();
    
    lastX = x;
    lastY = y;
}

function handleTouchStart(e) {
    e.preventDefault();
    const touch = e.touches[0];
    const rect = signatureCanvas.getBoundingClientRect();
    isDrawing = true;
    lastX = touch.clientX - rect.left;
    lastY = touch.clientY - rect.top;
}

function handleTouchMove(e) {
    e.preventDefault();
    if (!isDrawing) return;
    
    const touch = e.touches[0];
    const rect = signatureCanvas.getBoundingClientRect();
    const x = touch.clientX - rect.left;
    const y = touch.clientY - rect.top;
    
    signatureCtx.beginPath();
    signatureCtx.moveTo(lastX, lastY);
    signatureCtx.lineTo(x, y);
    signatureCtx.stroke();
    
    lastX = x;
    lastY = y;
}

function stopDrawing() {
    isDrawing = false;
}

window.clearSignatureCanvas = function(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    ctx.fillStyle = '#fff';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
};

window.getSignatureImage = function(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return null;
    
    return canvas.toDataURL('image/png');
};

window.getGeolocation = function() {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject('Geolocation not supported');
            return;
        }
        
        navigator.geolocation.getCurrentPosition(
            (position) => {
                resolve([position.coords.latitude, position.coords.longitude]);
            },
            (error) => {
                reject(error.message);
            },
            { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
        );
    });
};

window.triggerFileInput = function(inputId) {
    const input = document.getElementById(inputId);
    if (input) {
        input.click();
    }
};

let barcodeScanner = null;
let dotNetHelper = null;

window.initBarcodeScanner = async function(videoElementId, dotNetRef) {
    try {
        const video = document.getElementById(videoElementId);
        if (!video) {
            console.error('Video element not found');
            return false;
        }
        
        dotNetHelper = dotNetRef;
        
        const stream = await navigator.mediaDevices.getUserMedia({
            video: { facingMode: 'environment' }
        });
        
        video.srcObject = stream;
        barcodeScanner = stream;
        
        if ('BarcodeDetector' in window) {
            const detector = new BarcodeDetector({ formats: ['code_128', 'code_39', 'qr_code', 'ean_13'] });
            
            const detectLoop = async () => {
                if (!barcodeScanner) return;
                
                try {
                    const barcodes = await detector.detect(video);
                    if (barcodes.length > 0 && dotNetHelper) {
                        const code = barcodes[0].rawValue;
                        await dotNetHelper.invokeMethodAsync('OnBarcodeDetected', code);
                    }
                } catch (e) { }
                
                if (barcodeScanner) {
                    requestAnimationFrame(detectLoop);
                }
            };
            
            video.onloadedmetadata = () => {
                video.play();
                detectLoop();
            };
            
            return true;
        } else {
            console.warn('BarcodeDetector API not supported');
            return false;
        }
    } catch (error) {
        console.error('Barcode scanner error:', error);
        return false;
    }
};

window.stopBarcodeScanner = function() {
    if (barcodeScanner) {
        barcodeScanner.getTracks().forEach(track => track.stop());
        barcodeScanner = null;
    }
    dotNetHelper = null;
};

window.saveToIndexedDB = async function(storeName, data) {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open('Net4CourierPOD', 1);
        
        request.onerror = () => reject('IndexedDB error');
        
        request.onupgradeneeded = (event) => {
            const db = event.target.result;
            if (!db.objectStoreNames.contains('pendingPODs')) {
                db.createObjectStore('pendingPODs', { keyPath: 'offlineSyncId' });
            }
        };
        
        request.onsuccess = (event) => {
            const db = event.target.result;
            const transaction = db.transaction([storeName], 'readwrite');
            const store = transaction.objectStore(storeName);
            store.put(data);
            
            transaction.oncomplete = () => resolve(true);
            transaction.onerror = () => reject('Transaction error');
        };
    });
};

window.getFromIndexedDB = async function(storeName) {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open('Net4CourierPOD', 1);
        
        request.onerror = () => reject('IndexedDB error');
        
        request.onsuccess = (event) => {
            const db = event.target.result;
            if (!db.objectStoreNames.contains(storeName)) {
                resolve([]);
                return;
            }
            
            const transaction = db.transaction([storeName], 'readonly');
            const store = transaction.objectStore(storeName);
            const getAllRequest = store.getAll();
            
            getAllRequest.onsuccess = () => resolve(getAllRequest.result);
            getAllRequest.onerror = () => reject('GetAll error');
        };
    });
};

window.deleteFromIndexedDB = async function(storeName, key) {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open('Net4CourierPOD', 1);
        
        request.onerror = () => reject('IndexedDB error');
        
        request.onsuccess = (event) => {
            const db = event.target.result;
            const transaction = db.transaction([storeName], 'readwrite');
            const store = transaction.objectStore(storeName);
            store.delete(key);
            
            transaction.oncomplete = () => resolve(true);
            transaction.onerror = () => reject('Delete error');
        };
    });
};
