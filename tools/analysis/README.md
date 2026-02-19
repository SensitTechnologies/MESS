# NotebookLM Export Utility

This repository includes a developer utility script designed to export, categorize, and merge the MESS solution into structured text files suitable for ingestion by large language models (LLMs) such as NotebookLM.

This script is **not part of the .NET solution**. It is a standalone Python utility for code analysis and export.

---

## What This Script Does

The script performs the following steps:

### 1. Recursively Scans the Project

It walks the entire project directory starting from the directory the script is placed in.

It includes only selected source files:

* `.cs`
* `.razor`
* `.cshtml`
* `.json`
* `.xml`
* `.config`
* `.csproj`
* `.css`
* `.js`

It excludes:

* `bin/`
* `obj/`
* `.git/`
* `node_modules/`
* `lib/`
* `wwwroot/lib/` (third-party static libraries)

Images, fonts, and other static assets are ignored automatically because their file extensions are not included.

---

### 2. Extracts and Chunks Files

Each included file is:

* Read as UTF-8
* Prefixed with a header showing its relative path
* Counted for lines of code (raw line count)

If a file exceeds `MAX_CHARS` (180,000 characters), it is split into multiple chunk files.

---

### 3. Categorizes Files by Architecture Layer

Files are automatically grouped into categories based on folder names:

| Folder Match               | Category                |
| -------------------------- | ----------------------- |
| Pages, Components, Layouts | UI                      |
| Data, Migrations           | DataAccess / Migrations |
| Models, Entities           | Entities                |
| Services                   | Services                |
| DTOs                       | DTOs                    |
| Tests                      | Tests                   |
| Config, Infrastructure     | Config / Infrastructure |
| css                        | Styles                  |
| js                         | Scripts                 |
| wwwroot                    | Static                  |
| Other                      | Misc                    |

This allows the merged output to reflect architectural boundaries.

---

### 4. Creates Export Files

Two directories are generated in the same directory where the script is placed:

```
NotebookLM_export/
NotebookLM_merged/
```

#### NotebookLM_export

Contains individual processed text files (or file chunks).

Each exported file includes:

```
=== FILE: relative/path/to/file.cs ===
```

followed by the file contents.

---

#### NotebookLM_merged

Contains larger merged files grouped by category.

Example:

```
UI_part1.txt
Services_part1.txt
DataAccess_part1.txt
INDEX.txt
```

Files are merged up to approximately `MERGE_TARGET` (250,000 characters) per output file.

---

### 5. Generates an INDEX.txt

The merged directory includes an `INDEX.txt` file containing:

* File → merged chunk mapping
* Chunk size report (KB + file count)
* Lines of code per category
* Total project LOC

Example:

```
=== Project LOC Summary ===

UI: 15,302 LOC
Services: 8,041 LOC
DataAccess: 6,220 LOC
DTOs: 2,144 LOC

TOTAL: 31,707 LOC
```

LOC is calculated as a raw line count (newline count + 1 per file).
Blank lines and comments are included.

---

## Where To Place the Script

The script determines the project root using:

```python
PROJECT_ROOT = os.path.dirname(os.path.abspath(__file__))
```

This means:

> The script scans the directory it lives in.

### Important

To scan the MESS solution properly, place the script in:

```
MESS/
```

(the solution root that contains `MESS.sln`)

NOT inside:

* `MESS/MESS.Blazor`
* `MESS/MESS.Data`
* `MESS/MESS.Services`
* `MESS/MESS.Tests`

If placed in the wrong folder, it will only scan that subdirectory.

---

## How To Run

From the directory where the script is located:

```bash
python notebook_encoder.py
```

or on Windows:

```powershell
python .\notebook_encoder.py
```

You should see console output like:

```
Exported: NotebookLM_export/Program.cs.txt (220 LOC)
Merged: NotebookLM_merged/UI_part1.txt (243 KB, 18 files)

✅ Index created: NotebookLM_merged/INDEX.txt
✅ Total LOC: 48,833
```

---

## What Gets Created

Running the script will create (or reuse):

```
NotebookLM_export/
NotebookLM_merged/
```

If they already exist, files inside them will be overwritten.

The script does **not** delete previous files that are no longer generated.
If you want a clean export, delete those directories before running.

---

## Customization

You can adjust behavior at the top of the script:

* `INCLUDE_EXT` → control which file types are processed
* `EXCLUDE_DIRS` → directories to skip
* `MAX_CHARS` → max size per individual file chunk
* `MERGE_TARGET` → max size per merged output file
* `CATEGORY_MAP` → change architectural grouping

---

## Intended Use

This utility is designed for:

* Feeding the entire MESS codebase into NotebookLM
* LLM-assisted code review
* Architecture inspection
* LOC reporting
* Project size analysis
* AI-assisted refactoring discussions

It is not required for building or running MESS.

---

## Notes

* LOC is a raw line count (includes blank lines and comments).
* Third-party libraries inside `wwwroot/lib` are excluded.
* Output files are plain text and safe to inspect manually.
* The script does not modify any source files.
