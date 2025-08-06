export function printQRCode(value, index = null) {
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
                .qr-index {
                    margin-top: 8px;
                    font-size: 72px;
                    font-weight: bold;
                }
                @media print {
                    @page { margin: 0.5cm; }
                }
            </style>
        </head>
        <body>
            <img id="qr-img" src="${value}" alt="QR Code" />
            ${index !== null ? `<div class="qr-index">#${index}</div>` : ''}
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