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
export function scrollToStep(elementId) {
    const el = document.getElementById(elementId);
    if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
}

export function printRedTagById(id) {
    const tagElement = document.getElementById(id);
    if (!tagElement) return;

    const printWindow = window.open('', '_blank', 'width=500,height=300');

    // Get styles
    const styleSheets = [...document.styleSheets]
        .map(sheet => {
            try {
                return [...sheet.cssRules].map(rule => rule.cssText).join('\n');
            } catch (e) {
                return ''; // cross-origin styles
            }
        })
        .join('\n');

    printWindow.document.write(`
        <html>
        <head>
            <title>Print Red Tag</title>
            <style>${styleSheets}</style>
        </head>
        <body style="margin: 0; padding: 0;">
            ${tagElement.outerHTML}
            <script>
                window.onload = function() {
                    window.focus();
                    window.print();
                    window.onafterprint = () => window.close();
                };
            <\/script>
        </body>
        </html>
    `);
    printWindow.document.close();
}

