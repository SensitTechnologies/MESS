## Overview
The primary function of MESS is the creation of production logs, which allow for: 
* Part labeling and tracking
* Logging operators and their assembly processes
* Indication of success/failure for each step in the assembly process, helping with quality and error management

<img width="3024" alt="image" src="https://github.com/user-attachments/assets/7ddafb1b-b084-436f-94f2-93ad98e619ee" />

## Product and Work Instruction Select

Each product is associated with a set of work instructions. Upon selecting a product, the operator must select which set of instructions they are currently working on for their step in the assembly process. More details can be found in the [Product Management](https://github.com/SensitTechnologies/MESS/wiki/Product-Management) page.

---

## Batch Mode

Batch Mode allows operators to complete and log **multiple units of the same product** in a single production run without needing to reset or reselect products and work instructions for each unit.

<img width="3024" alt="image" src="https://github.com/user-attachments/assets/43879bf0-64c8-47c4-a8e9-5698619f0fac" />

When Batch Mode is enabled:

* **Single Setup, Multiple Logs** – The operator selects the product and work instructions once, then proceeds to record results for each unit in sequence.
* **Automatic Log Creation** – After recording the success/failure for one unit, the system automatically advances to the next without returning to the product selection screen.
* **Consistent Context** – Operator, product, and work instruction details remain locked in until the batch is finished, ensuring data consistency.
* **Batch Summary** – At the end of the run, the operator can review all units’ results before final submission.

---

### UI Behavior

When **Batch Mode** is active, the step logging interface changes to support **multi-step control** for a single Work Instruction step across multiple production log entries:

1. **Pass All / Fail All Buttons**

   * Instantly marks **all units in the batch** for the current step as Success or Failure.
   * Displays a floating **"Pass All!"** or **"Fail All!"** confirmation message.
   * On failure, automatically shows an **optional notes field** for entering a defect description, and allows triggering a **Red Tag** dialog.

2. **Indexed Control**

   * Allows selecting an individual unit from the batch by **step index**.
   * The selected index can be changed using a number input field (auto-clamped between `1` and the batch size).
   * Success/Failure can be recorded for that specific unit without affecting others.
   * Displays **"Success!"** or **"Failure!"** floating labels for that unit.

3. **Optional Failure Notes**

   * If a unit fails, the operator can add failure notes in real time.
   * Notes are bound to the specific `ProductionLogStepAttempt` for that unit and updated immediately in the database via `ProductionLogEventService`.

4. **Red Tag Integration**

   * Operators can open the **Red Tag dialog** from the step view to create a defect tag for a failed attempt.
   * If a failure attempt is new and unsaved, the system attempts to trigger a DB save before opening the dialog.

---

### Technical Notes

* In **One Piece Flow**, `LogSteps` will contain only **one** `ProductionLogStep`.
  In **Batch Mode**, `LogSteps` contains **multiple** entries (one per unit) for the same step.
* The UI automatically detects whether all units share the same Success/Failure state, enabling the corresponding batch button highlight.
* The component uses:

  * **`HandleButtonPressAll`** – Sets result for all units in batch.
  * **`HandleButtonPressIndexed`** – Sets result for a single indexed unit.
  * **`ShowFloatMessage`** – Displays transient visual confirmation for user actions.
* Detailed or optional content is toggle-able via **Show/Hide Details**, and media can be displayed in a carousel.

## Success/Failure Indicators
Each work instruction step is paired with **success** and **failure** buttons that allow operators to indicate whether the step was completed successfully. If **success** is pushed, the data is saved and stored. If **failure** is pushed, operators are given the option to write a note on what went wrong and the information is saved. 

## Detail and Image Display
Operators are provided the optional ability to display more detailed steps for the assembly process if necessary. Optional image carousels are also provided as visual guidance during assembly.

## Log Submission
Upon submitting a log, the page will refresh and the operator will be able to start a new assembly process without manually navigating to a new log. The data associated with the log (i.e. operator, product, successes and failures) is then stored in the database.

## QR Code Generation
Some submitted logs will generate a QR code to associate with a set of work instructions for traceability purposes.  