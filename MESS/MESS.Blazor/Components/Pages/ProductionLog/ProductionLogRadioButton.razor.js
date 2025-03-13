export function printQRCode(value) {
    const iframe = document.createElement('iframe');
    iframe.style.display = 'none';
    document.body.appendChild(iframe);

    iframe.contentDocument.write(`
        <html>
        <head>
            <style>
                body {
                    margin: 0;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                }
                @media print {
                    @page { margin: 0.5cm; }
                }
            </style>
        </head>
        <body>
            <img src="${value}" alt="QR Code" onload="window.focus(); window.print(); setTimeout(window.frameElement.remove.bind(window.frameElement), 100);" />
        </body>
        </html>
    `);
    
    iframe.contentDocument.close();
}