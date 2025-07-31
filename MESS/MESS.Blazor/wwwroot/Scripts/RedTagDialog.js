export function printRedTagById(id) {
    const tagElement = document.getElementById(id);
    if (!tagElement) return;

    const dateField = tagElement.querySelector('#tag-date');
    if (dateField) {
        dateField.textContent = new Date().toLocaleDateString();
    }

    const clone = tagElement.cloneNode(true);

    function removeFixedSizes(element) {
        if (element.style) {
            element.style.width = '';
            element.style.height = '';
            element.style.margin = '';
            element.style.padding = '';
        }
        for (const child of element.children) {
            removeFixedSizes(child);
        }
    }
    removeFixedSizes(clone);

    const hole = clone.querySelector('.red-tag-hole');
    if (hole) hole.remove();

    const printWindow = window.open('', '_blank', 'width=900,height=450');

    const printStyles = `
    @page {
        size: 7.8in 2.15in;
        margin: 0.08in;
    }
    html, body {
        margin: 0;
        padding: 0;
        width: 7.64in;
        height: 1.99in;
        font-family: Arial, sans-serif;
        font-size: 12.8pt;
        color: black;
        background: white;
        box-sizing: border-box;
        line-height: 1.25;
    }
    .red-tag-wrapper {
        padding: 0.1in 0.2in 0 0.2in;
        box-sizing: border-box;
    }
    .row {
        display: flex;
        flex-direction: row;
        flex-wrap: wrap;
        gap: 1.5em;
        margin-bottom: 0.2em;
    }
    .field {
        white-space: nowrap;
        flex-shrink: 0;
    }
    .failure-note {
        margin-top: 0.25in;
        display: flex;
        flex-wrap: wrap;
    }
    .failure-note .failure-text {
        color: #b00000;
        word-break: break-word;
        flex: 1 1 auto;
        min-width: 0;
    }
    `;

    function extractValue(item) {
        const label = item.querySelector('.label');
        const value = Array.from(item.childNodes)
            .filter(n => n !== label)
            .map(n => n.textContent.trim())
            .join(' ');
        return value;
    }

    function buildLayout(root) {
        const tagItems = Array.from(root.querySelectorAll('.tag-info-grid .tag-item'));
        const map = {};

        tagItems.forEach(item => {
            const label = item.querySelector('.label');
            if (!label) return;
            const key = label.textContent.replace(':', '').trim();
            map[key] = extractValue(item);
        });

        const failureNoteEl = root.querySelector('.failure-note');
        const failureLabel = failureNoteEl?.querySelector('.label');
        const failureText = Array.from(failureNoteEl?.childNodes || [])
            .filter(n => n !== failureLabel)
            .map(n => n.textContent.trim())
            .join(' ') || '';

        const wrapper = document.createElement('div');
        wrapper.className = 'red-tag-wrapper';

        const row1 = document.createElement('div');
        row1.className = 'row';
        row1.innerHTML = `
            <span class="field">RED TAG</span>
            <span class="field">${map['Date'] || ''}</span>
            <span class="field">${map['ID'] || ''}</span>
        `;

        const row2 = document.createElement('div');
        row2.className = 'row';
        row2.innerHTML = `
            <span class="field">${map['Product'] || ''}</span>
            <span class="field">${map['Work Instruction'] || ''}</span>
            <span class="field">${map['Step'] || ''}</span>
        `;

        const row3 = document.createElement('div');
        row3.className = 'row';
        row3.innerHTML = `<span class="field">${map['Operator'] || ''}</span>`;

        const failureDiv = document.createElement('div');
        failureDiv.className = 'failure-note';
        failureDiv.innerHTML = `
            <span class="failure-text">${failureText}</span>
        `;

        wrapper.appendChild(row1);
        wrapper.appendChild(row2);
        wrapper.appendChild(row3);
        wrapper.appendChild(failureDiv);

        return wrapper.outerHTML;
    }

    const printableHTML = buildLayout(clone);

    printWindow.document.write(`
    <!DOCTYPE html>
    <html>
    <head>
      <title>Print Red Tag</title>
      <style>${printStyles}</style>
    </head>
    <body>
      ${printableHTML}
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
