## Overview

The **Products** page is where all product records within MESS are created, versioned, and managed.
From here, authorized users can:

* Create brand new products from scratch.
* Generate new versions of existing products.
* Activate or deactivate products for production visibility.
* Review all stored product information at a glance.

Below is a screenshot of the **Products** page.

<img width="3024" alt="image" src="https://github.com/user-attachments/assets/1f4f31f5-7c9e-4398-8481-0b9f107a8478" />

---

## Editing Products

The product table supports **inline editing** for all product fields.
However, edits to existing products cannot be saved in place — they must be committed by creating a **new product record** based on the changes.

**Key points:**

* This approach prevents altering or deleting products that may be referenced in production logs.
* Product visibility to operators is controlled by the **Active** checkbox.
* Saving changes to an existing product always produces a separate version, preserving the original.

---

### Managing Work Instruction Associations

Each product has a column showing all available **work instructions** from the database.
Here, users can:

* Select or deselect instructions to link them with the product.
* Adjust associations without impacting other product properties.

---

## Creating New Products

The last row in the table is always blank and acts as a **creation form**.
When filled out and confirmed via the **Create Product** button:

* A new product record is generated.
* Work instruction associations can be set immediately after creation.

---

## How the Component Works

The UI for each row in the product table is built as a **self-contained Blazor component**.
It handles both **display** and **interaction logic** for that product.

**General behaviors:**

* Tracks whether a row has unsaved changes.
* Disables repeated save clicks while an operation is in progress.
* Displays different buttons depending on whether the row is an existing product or a new one.
* Uses a **confirmation dialog** for destructive actions like deletion.
* Delegates all actual save, delete, and association operations to parent-level logic via callbacks.

---

### Feature Highlights

**1. Inline Editing**
Product name and active status can be changed directly in the table row.
Any edit marks the row as “dirty,” enabling save actions.

**2. Conditional Actions**

* Existing products show **Save** and **Delete** controls.
* New products show a **Create** control instead.

**3. Work Instruction Selector**
Each row integrates a selection UI to manage product–instruction links:

* Supports adding new instructions to the product.
* Supports removing selected instructions.
* Works in sync with the product's own data.

**4. Confirmation Before Deletion**
Deletes require explicit user confirmation via a modal dialog, reducing risk of accidental removal.

**5. Parent–Child Communication**
Instead of saving or deleting directly, the row component raises **events** that the parent table component listens to.
This allows:

* Centralized data handling.
* Server communication logic to remain outside the row component.