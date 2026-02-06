window.pdfPreviewHelper = {
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
    downloadPdf: function (url, title) {
        var a = document.createElement('a');
        a.href = url;
        a.download = (title || 'document') + '.pdf';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    },
    setupLoadTimeout: function (iframeElement, dotNetRef, timeoutMs) {
        setTimeout(function () {
            try {
                var doc = iframeElement.contentDocument || iframeElement.contentWindow.document;
                if (!doc || doc.contentType !== 'application/pdf') {
                    var bodyText = doc && doc.body ? doc.body.innerText : '';
                    if (bodyText && bodyText.indexOf('Error') >= 0) {
                        dotNetRef.invokeMethodAsync('OnLoadError');
                    }
                }
            } catch (e) {
            }
        }, timeoutMs || 10000);
    }
};
