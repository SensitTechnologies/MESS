### Important Information
* 
***
### Overview
Below is a screenshot of the **Work Instruction Import** Dialog Box.

<img width="3024" alt="image" src="https://github.com/user-attachments/assets/27fda490-5d3f-407e-b031-18c6b0ece76c" />

#### Work Instruction Structure
The Work Instruction within MESS has the following structure:

* **ID**: Generated automatically by the database provider.
* **Title**: The Title of the WorkInstruction.
* **Version History ID**: The Version. Commonly used to differentiate between altered types of Work Instructions.
* **IsActive**: Boolean value dictates whether or not a Work Instruction is available to Operators.
* **PartsRequired**: Boolean value dictates whether or not Parts are required. (Import related)
* **Nodes**: A List of Nodes, currently can only be a PartNode, or a Step and is used for ordering the Instruction dynamically.
* **Products**: A List of associated Products.
***
#### Fields
Currently Technicians are able to edit the following fields:
* Title
* Version
* Action Order

***

#### Colors
Currently MESS will not apply any CSS color attribute if the color is deemed to be **Black**. This is done since there have been scenarios where a cell will only have bold or italics applied, yet it has an arbitrary color value applied. This then disables dark/light mode for that cell in particular.

***
### Creating a Work Instruction Import Spreadsheet

Using a [sample spreadsheet from the MESS repository](https://github.com/SensitTechnologies/MESS/blob/7d6d1c5fa1d880b9e8014caa3e1bf54c56ed8283/templates/Cook%20Beef%20Stew.xlsx), you can edit it with any XLSX-compatible spreadsheet editor to create your own work instructions for import.

It is important to remember:

* **Pictures must be placed in one of the two media columns, not over them**.
* You should be able to freely drag the image.
* The image’s **top-left corner** must be positioned over the media cell of your choice.
* Multiple images can be placed in the same cell, and you can include more than two pictures for a single production log step.

**Steps:**

1. Go to the [sample spreadsheet folder](https://github.com/SensitTechnologies/MESS/blob/7d6d1c5fa1d880b9e8014caa3e1bf54c56ed8283/templates/Cook%20Beef%20Stew.xlsx) in the MESS repository.
2. Download and edit the sample spreadsheet.
3. Ensure all images follow the placement rules above.
4. Save your file in `.xlsx` format before importing.

The Work Instruction Spreadsheet within MESS has the following structure:

* **Products**: Name of product that this work instruction contributes to/associated with.
* **Instruction**: Title of this set of steps to produce the product.
* **Version**: Version code for this document.
* **QR Code**: Set to true if the work instruction produces a printed QR code on completion. Set to false otherwise.
* **Serial Number**: Set to true if the work instruction allows the operator to input a product serial number after the last step. Set to false otherwise.
* **Version History ID**: Optional integer field that can be used to link this work instruction to an already existing version history chain. Leave blank if unsure. 
***
#### Known Limitations
* Spreadsheet Import does not currently allow Support of Importing Spreadsheets with Multiple Associated Products

***