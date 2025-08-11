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
        width: 7.64in; /* 7.8 - 2*0.08 */
        height: 1.99in; /* 2.15 - 2*0.08 */
        font-family: Arial, sans-serif;
        font-size: 17pt;
        color: black;
        background: white;
        box-sizing: border-box;
        line-height: 1.10;
        display: flex;
        align-items: center;
        justify-content: center;
    }
    .red-tag-wrapper {
        width: 100%;
        height: 100%;
        padding: 0;
        display: flex;
        flex-direction: column;
        justify-content: space-evenly;
        box-sizing: border-box;
    }
    .row {
        display: flex;
        flex-wrap: nowrap;
        justify-content: space-between;
        gap: 0.2in;
        margin-bottom: 0.05in;
    }
    .field {
        flex-shrink: 0;
        white-space: nowrap;
        font-weight: bold;
    }
    .failure-note {
        display: flex;
        margin-top: 0.1in;
    }
    .failure-text {
        color: #b00000;
        word-break: break-word;
        flex: 1;
        font-weight: normal;
    }
    `;

    function extractValue(item) {
        const clone = item.cloneNode(true);
        const label = clone.querySelector('.label');
        if (label) label.remove();
        return clone.textContent.trim();
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
        let failureText = '';
        if (failureNoteEl) {
            const failureClone = failureNoteEl.cloneNode(true);
            const label = failureClone.querySelector('.label');
            if (label) label.remove();
            failureText = failureClone.textContent.trim();
        }

        const wrapper = document.createElement('div');
        wrapper.className = 'red-tag-wrapper';

        const row1 = document.createElement('div');
        row1.className = 'row';
        row1.innerHTML = `
            <span class="field">RED TAG</span>
            <span class="field">${map['Date'] || ''}</span>
            <span class="field">${map['Operator'] || ''}</span>
            <span class="field">${map['ID'] || ''}</span>
        `;

        const row2 = document.createElement('div');
        row2.className = 'row';
        row2.innerHTML = `
            <span class="field">${map['Product'] || ''}</span>
            <span class="field">${map['Work Instruction'] || ''}</span>
            <span class="field">${map['Step'] || ''}</span>
        `;

        const failureDiv = document.createElement('div');
        failureDiv.className = 'failure-note';
        failureDiv.innerHTML = `<span class="failure-text">${failureText}</span>`;

        wrapper.appendChild(row1);
        wrapper.appendChild(row2);
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
