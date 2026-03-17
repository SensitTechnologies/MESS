## Overview

The **Red Tag** component provides a printable defect tag for tracking and documenting failed production attempts.
It integrates with authentication, product/work instruction context, and JavaScript printing to produce a physical red tag label containing operator, product, and defect details.

Currently, printing is performed using a Brother PT-P700 label printer using ~1 inch/24mm laminated TZe-252 label tape.

<img width="2596" alt="image" src="https://github.com/user-attachments/assets/572789e2-13b1-4e43-a6f7-7ad140504352" />

---

## Main Features

### 1. Tag Information Display

Displays key defect tracking fields:

* **RED TAG** label
* **Date** (attempt timestamp)
* **ID** (attempt ID)
* **Product Name**
* **Part Name** / **Part #** (optional fields)
* **Work Instruction** (associated with the defect)
* **Step** (where defect occurred)
* **Operator** (retrieved from authentication claims)
* **Failure Note** (operator-entered description of issue)

---

### 2. Operator Name Resolution

When initializing:

* Attempts to get **First Name** and **Last Name** from authentication claims.
* Falls back to:

  * `"name"` claim value, or
  * Identity name, or
  * `"Unknown Operator"` if none found.

---

### 3. Printing Workflow

* **Triggered by:** Clicking the **Print** button.
* **Process:**

  1. Calls JavaScript function `printRedTagById` with tag element ID.
  2. Clones and cleans the DOM for print formatting.
  3. Removes fixed sizing & non-essential elements (e.g., hole graphic).
  4. Builds a compact print layout with key fields.
  5. Opens print dialog in a new browser window.
  6. Automatically closes print window after printing.

---

### 4. JavaScript Print Module Features

* Dynamically formats **print-specific layout** and CSS:

  * Paper size: `7.8in x 2.15in`
  * Minimal margins
  * Bold labels
  * Highlighted failure note
* Automatically sets the **Date** field.
* Ensures **word wrapping** for long failure notes.

---

### 5. Buttons & Actions

* **Cancel:** Triggers `OnCancel` callback (parent component can close dialog).
* **Print:** Executes print workflow, then triggers `OnSubmit` callback.

---
