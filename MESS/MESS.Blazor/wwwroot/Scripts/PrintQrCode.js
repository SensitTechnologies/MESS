window.printQRCode = (dataUrl, label = null) => {
    const iframe = document.createElement('iframe');
    iframe.style.display = 'none';
    document.body.appendChild(iframe);

    const doc = iframe.contentDocument || iframe.contentWindow.document;

    doc.open();
    doc.write(`
        <html>
        <head>
            <style>
                body {
                    margin: 0;
                    display: flex;
                    flex-direction: column;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                    font-family: sans-serif;
                }
                .qr-label {
                    margin-top: 12px;
                    font-size: 48px;
                    font-weight: bold;
                    text-align: center;
                }
                @media print {
                    @page { margin: 0.5cm; }
                }
            </style>
        </head>
        <body>
            <img id="qr-img" src="${dataUrl}" alt="QR Code" />
            ${label ? `<div class="qr-label">${label}</div>` : ''}
            <script>
                const img = document.getElementById('qr-img');
                img.onload = function () {
                    window.focus();
                    window.print();
                    setTimeout(() => window.frameElement?.remove(), 100);
                };
            </script>
        </body>
        </html>
    `);
    doc.close();
}