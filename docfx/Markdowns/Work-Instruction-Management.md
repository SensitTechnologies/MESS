## Overview

The **Work Instruction Manager** page provides a complete UI for creating, editing, deleting, importing, and managing **Work Instructions** associated with products.

It integrates **role-based access control**, **dynamic filtering**, **node management**, **versioning**, and **JavaScript interop** for smooth editing and navigation.

Below is a screenshot of the **Work Instruction Management** page.

<img width="3024" alt="image" src="https://github.com/user-attachments/assets/20597a94-7359-4890-8905-228ee8bb4af8" />

---

## Access Control

**Roles Allowed:**

* `Technician`
* `Administrator`

> Users without these roles cannot access this page.

---

## Main Features

### 1. Product & Work Instruction Management

**Loads:**

* Active Products via `ProductService`.
* Latest Work Instructions via `WorkInstructionService`.

**Filter:**

* Work instructions by selected product.

**Supports:**

* Creating new instructions.
* Duplicating (“Save As”) instructions.
* Editing product associations.
* Allowing technicians to import pictures.
* Importing instructions.
* Restoring from version history.
* IsActive: Boolean value dictates whether or not a Work Instruction is available to Operators.
* Nodes: A List of Nodes, currently can only be a PartNode, or a Step and is used for ordering the Instruction dynamically.

---

### 2. Interactive UI Components

#### **MenuBarPhoebe**

* Product and instruction selection.
* Sidebar toggle.
* Create, delete, save, “Save As”, import, restore actions.
* Add new Part/Step.

#### **NavigationMenu**

* Toggleable sidebar navigation.

#### **WorkInstructionNodeManagerList**

* Displays and manages nodes (Parts and Steps).
* Supports adding/removing nodes dynamically.

---

### 3. Node Editing

**Add:**

* Part nodes (with part name & number).
* Step nodes (with name, body, detailed body).

**Insert at:**

* End of list.
* Specific index.

**Other:**

* Remove nodes (queued for deletion).
* Auto-update positions and scroll to new/modified nodes.

---

### 4. Saving & Persistence

* Save changes to:

  * Existing instructions (if editable).
  * New instructions.
  * New versions from existing ones.
* Validates that the instruction has a title.
* Refreshes product and instruction lists after saving.

---

### 5. Deleting Instructions

* Deletes all versions of an instruction.
* Confirmation dialog shown if the instruction has been used in production.
* Updates product and instruction lists after deletion.

---

### 6. Importing Instructions

* Adds imported instructions to the list.
* Resets product filter to “All Products”.
* Automatically loads imported instruction into the editor.

---

### 7. User Feedback

**Toasts:**

* Success (green).
* Error (red).
* Info (blue).

**Alerts:**

* Shown when instructions have no parts or steps.

---

### 8. JavaScript Interop

* Imports and uses `ScrollTo.js` to automatically scroll to newly added or updated nodes.

---

## Actions

The **Actions** of a Work Instruction are the individual items containing content for the Operator to view and interact with.
Currently, MESS supports two types of Actions: **Step** and **Part**.

### Step

A **Step** is the primary Action in MESS and contains both **Primary** and **Secondary** content in text format. It also includes **Failure/Success** buttons to record the status of that step.

> **Note:** These status values are saved to the database. If an Operator selects Success or Failure, deselects it, and selects again, the most recent timestamp is recorded.

#### Primary Content

Primary content is **always** shown to the Operator.

#### Secondary Content (Optional)

Secondary content—sometimes referred to as *optional*—can be shown or hidden by the Operator using an Expand/Collapse button on the Production Log creation page.

Both Primary and Secondary content can include **text, images, hyperlinks**, and **rich text**.

#### Operator Choices

Each Step includes a Failure/Success button set for operator input.

---

### Part

A **Part** Action contains a list of part objects where Operators can enter serial numbers for additional data collection.

---

## How It Works

### Initialization

**`OnInitializedAsync`**

* Subscribes to `WorkInstructionEditorService.OnChanged`.
* Loads products and latest instructions.

---

### First Render

**`OnAfterRenderAsync`**

* Loads JS scroll-to module.

---

### Selection Flow

**Product Selected**

* Filters work instruction list.
* Resets editor.

**Work Instruction Selected**

* Loads into editor.
* Removes placeholder unsaved instructions.

---

### Editing Flow

* Adding/removing parts or steps updates:

  * Node positions.
  * Dirty state.
  * UI (via `StateHasChanged`).
* Deleting queues node for removal from service.

---

### Saving Flow

1. Validate title.
2. Check editability via `WorkInstructionService.IsEditable`.
3. Decide save mode:

   * Update existing.
   * Save as new.
   * Create new version.
4. Show success/error toast.
5. Refresh product and instruction lists.

---

### Deleting Flow

1. If instruction has production usage → show confirmation dialog.
2. If confirmed or unused → delete all versions.
3. Refresh lists and reset editor.