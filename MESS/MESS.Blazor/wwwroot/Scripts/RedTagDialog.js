export function printRedTagById(id) {
    const tagElement = document.getElementById(id);
    if (!tagElement) return;

    const clone = tagElement.cloneNode(true);
    clone.classList.remove('red-tag-print-source');

    const printWindow = window.open('', '_blank', 'width=900,height=450');

    const printStyles = `
    @page {
        size: 7.8in 2.15in;
        margin: 0.08in;
    }
    html, body {
        margin: 0;
        padding: 0;
        width: 7.64in; /* 7.8 - 2*0.08 */
        height: 1.99in; /* 2.15 - 2*0.08 */
        font-family: monospace, monospace;
        font-size: 12pt;
        color: #000;
        background: white;
        box-sizing: border-box;
        line-height: 1.8;
    }
    .red-tag-print-wrapper {
        display: block;
    }
    .red-tag-print {
        font-family: monospace, monospace;
        font-size: 12pt;
        color: #000;
        padding: 0;
        margin: 0;
        line-height: 1.8;
    }
    .red-tag-print .line-header {
        font-weight: bold;
        margin: 0 0 4pt;
    }
    .red-tag-print .line {
        margin: 0 0 2pt;
    }
    .red-tag-print .label {
        font-weight: bold;
        margin-right: 4pt;
    }
    .red-tag-print .value {
        font-weight: normal;
    }
    `;

    printWindow.document.write(`
    <!DOCTYPE html>
    <html>
    <head>
      <title>Print Red Tag</title>
      <style>${printStyles}</style>
    </head>
    <body>
      ${clone.outerHTML}
      <script>
        window.onload = function () {
          window.focus();
          window.print();
          window.onafterprint = () => window.close();
        };
      </script>
    </body>
    </html>
    `);

    printWindow.document.close();
}
