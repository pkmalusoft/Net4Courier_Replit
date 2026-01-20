window.downloadFile = function (fileName, base64Content, mimeType) {
    const link = document.createElement('a');
    link.href = 'data:' + mimeType + ';base64,' + base64Content;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.downloadFileFromBase64 = function (base64Content, fileName, mimeType) {
    const link = document.createElement('a');
    link.href = 'data:' + mimeType + ';base64,' + base64Content;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
