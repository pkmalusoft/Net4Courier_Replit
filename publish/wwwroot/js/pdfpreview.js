window.pdfPreviewHelper = {
    fetchAndDisplayPdf: async function (iframeElement, url, dotNetRef) {
        try {
            var response = await fetch(url, {
                credentials: 'same-origin',
                headers: { 'Accept': 'application/pdf' }
            });
            if (!response.ok) {
                var errorMsg = 'Server returned status ' + response.status;
                try {
                    var text = await response.text();
                    if (text) {
                        try {
                            var json = JSON.parse(text);
                            errorMsg = json.detail || json.title || json.message || errorMsg;
                        } catch (e2) {
                            if (text.length < 500) errorMsg = text;
                        }
                    }
                } catch (e3) { }
                if (dotNetRef) dotNetRef.invokeMethodAsync('OnLoadErrorWithMessage', errorMsg);
                return null;
            }
            var contentType = response.headers.get('content-type') || '';
            if (contentType.indexOf('text/html') >= 0) {
                if (dotNetRef) dotNetRef.invokeMethodAsync('OnLoadErrorWithMessage', 'Server returned HTML instead of PDF');
                return null;
            }
            var blob = await response.blob();
            var blobUrl = URL.createObjectURL(blob);
            if (iframeElement) {
                iframeElement.src = blobUrl;
            }
            return blobUrl;
        } catch (e) {
            var msg = e.message || 'Network error loading document';
            if (dotNetRef) dotNetRef.invokeMethodAsync('OnLoadErrorWithMessage', msg);
            return null;
        }
    },
    printPdf: function (iframeElement) {
        try {
            if (iframeElement && iframeElement.contentWindow) {
                iframeElement.contentWindow.focus();
                iframeElement.contentWindow.print();
            }
        } catch (e) {
            window.open(iframeElement.src, '_blank');
        }
    },
    downloadPdf: async function (url, title) {
        try {
            var response = await fetch(url, {
                credentials: 'same-origin',
                headers: { 'Accept': 'application/pdf' }
            });
            if (response.ok) {
                var blob = await response.blob();
                var blobUrl = URL.createObjectURL(blob);
                var a = document.createElement('a');
                a.href = blobUrl;
                a.download = (title || 'document') + '.pdf';
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(blobUrl);
            }
        } catch (e) {
            var a = document.createElement('a');
            a.href = url;
            a.download = (title || 'document') + '.pdf';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
        }
    },
    revokeBlobUrl: function (blobUrl) {
        if (blobUrl) {
            try { URL.revokeObjectURL(blobUrl); } catch (e) { }
        }
    }
};
