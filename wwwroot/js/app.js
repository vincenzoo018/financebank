// FineBank Application JavaScript Functions

// Download file from Base64 data
window.downloadFile = function (fileName, contentType, base64Data) {
    const link = document.createElement('a');
    link.href = `data:${contentType};base64,${base64Data}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Download HTML as file (for contract)
window.downloadHtmlAsFile = function (fileName, htmlContent) {
    const blob = new Blob([htmlContent], { type: 'text/html' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

// Print contract content within an iframe (MAUI-compatible)
window.printContractInline = function (htmlContent) {
    // Create a hidden iframe for printing
    let iframe = document.getElementById('contract-print-frame');
    if (!iframe) {
        iframe = document.createElement('iframe');
        iframe.id = 'contract-print-frame';
        iframe.style.cssText = 'position: fixed; right: 0; bottom: 0; width: 0; height: 0; border: 0;';
        document.body.appendChild(iframe);
    }
    
    const iframeDoc = iframe.contentDocument || iframe.contentWindow.document;
    iframeDoc.open();
    iframeDoc.write(htmlContent);
    iframeDoc.close();
    
    // Wait for content to load then print
    setTimeout(function() {
        iframe.contentWindow.focus();
        iframe.contentWindow.print();
    }, 500);
};

// Print current page
window.printPage = function () {
    window.print();
};

// Open contract in new window for printing/PDF download
window.openContractWindow = function (htmlContent) {
    const printWindow = window.open('', '_blank', 'width=900,height=700');
    if (!printWindow) {
        alert('Please allow pop-ups to download the contract PDF.');
        return;
    }
    
    printWindow.document.write(htmlContent);
    printWindow.document.close();
    
    // Add print button to the top of the document
    const printBar = printWindow.document.createElement('div');
    printBar.className = 'no-print';
    printBar.style.cssText = 'position: fixed; top: 0; left: 0; right: 0; background: linear-gradient(135deg, #059669, #10b981); padding: 15px 20px; display: flex; justify-content: center; gap: 15px; z-index: 9999; box-shadow: 0 4px 12px rgba(0,0,0,0.2);';
    
    const printBtn = printWindow.document.createElement('button');
    printBtn.innerHTML = '🖨️ Print / Save as PDF';
    printBtn.style.cssText = 'background: white; color: #059669; border: none; padding: 12px 30px; border-radius: 8px; cursor: pointer; font-weight: 700; font-size: 14px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);';
    printBtn.onclick = function() { printWindow.print(); };
    
    const closeBtn = printWindow.document.createElement('button');
    closeBtn.innerHTML = '✕ Close';
    closeBtn.style.cssText = 'background: rgba(255,255,255,0.2); color: white; border: 2px solid white; padding: 12px 30px; border-radius: 8px; cursor: pointer; font-weight: 600; font-size: 14px;';
    closeBtn.onclick = function() { printWindow.close(); };
    
    printBar.appendChild(printBtn);
    printBar.appendChild(closeBtn);
    printWindow.document.body.insertBefore(printBar, printWindow.document.body.firstChild);
    
    // Add padding to top of content to account for fixed bar
    const container = printWindow.document.querySelector('.container');
    if (container) {
        container.style.marginTop = '80px';
    }
};