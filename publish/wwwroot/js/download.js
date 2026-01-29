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

window.downloadFileFromStream = function (fileName, base64Content) {
    const link = document.createElement('a');
    link.href = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,' + base64Content;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.downloadFileFromBytes = function (fileName, base64Content) {
    const link = document.createElement('a');
    link.href = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,' + base64Content;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.printContent = function (htmlContent) {
    var printWindow = window.open('', '_blank');
    if (printWindow) {
        printWindow.document.write(htmlContent);
        printWindow.document.close();
        printWindow.focus();
        setTimeout(function() {
            printWindow.print();
        }, 250);
    }
};
