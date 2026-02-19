import os
from datetime import datetime
from collections import defaultdict

# --- CONFIG ---
PROJECT_ROOT = os.path.dirname(os.path.abspath(__file__))  # auto-detect solution root
EXPORT_DIR   = os.path.join(PROJECT_ROOT, "NotebookLM_export")
MERGED_DIR   = os.path.join(PROJECT_ROOT, "NotebookLM_merged")

# file extensions to include
INCLUDE_EXT  = {".cs", ".razor", ".cshtml", ".json", ".xml", ".config", ".csproj", ".css", ".js"}
EXCLUDE_DIRS = {"bin", "obj", ".git", "node_modules", "lib"}  

MAX_CHARS   = 180000   # per-file chunk size (before merge)
MERGE_TARGET = 250000  # target size per merged file

CATEGORY_MAP = {
    # UI / Blazor
    "pages": "UI",
    "components": "UI",
    "layouts": "UI",
    "dialogs": "UI",
    "managers": "UI",
    "razor": "UI",   # catchall for razor if not in a specific folder

    # EF Core Data layer
    "data": "DataAccess",
    "migrations": "Migrations",
    "entities": "Entities",
    "models": "Entities",
    "seeds": "DataAccess",

    # Services
    "services": "Services",
    "dtos": "DTOs",
    "dto": "DTOs",
    "mapper": "DTOs",

    # Tests
    "tests": "Tests",

    # Config / Infra
    "config": "Config",
    "properties": "Config",
    "infrastructure": "Infrastructure",
    "cache": "Infrastructure",
    "session": "Infrastructure",
    "role": "Infrastructure",
    "user": "Infrastructure",

    # Styling + JS
    "css": "Styles",
    "js": "Scripts",
    "wwwroot": "Static",
}

def should_include_file(path: str) -> bool:
    _, ext = os.path.splitext(path)
    ext = ext.lower()

    if ext not in INCLUDE_EXT:
        return False

    # Explicitly skip anything under wwwroot/lib
    if "wwwroot/lib/" in path.replace("\\", "/").lower():
        return False

    return True

def safe_filename(name: str) -> str:
    return name.replace(":", "_").replace("\\", "_").replace("/", "_")

def process_file(file_path: str, rel_path: str):
    try:
        with open(file_path, "r", encoding="utf-8", errors="ignore") as f:
            content = f.read()
    except Exception as e:
        print(f"Skipping {file_path}: {e}")
        return [], 0

    header = f"=== FILE: {rel_path} ===\n\n"
    full_text = header + content

    # count lines of code
    loc = content.count("\n") + 1

    if len(full_text) <= MAX_CHARS:
        return [(safe_filename(rel_path) + ".txt", full_text)], loc

    chunks = []
    for i, start in enumerate(range(0, len(full_text), MAX_CHARS)):
        chunk_text = full_text[start:start + MAX_CHARS]
        chunks.append((safe_filename(rel_path) + f"_part{i+1}.txt", chunk_text))
    return chunks, loc

def categorize_file(rel_path: str) -> str:
    # normalize to lowercase path parts
    parts = [p.lower() for p in rel_path.replace("\\", "/").split("/")]

    for key, cat in CATEGORY_MAP.items():
        # match only against full folder/file parts (not substrings inside words)
        if any(p == key or p.endswith(key) for p in parts):
            return cat
    return "Misc"

def export_project():
    os.makedirs(EXPORT_DIR, exist_ok=True)
    all_files = []
    total_loc = 0
    loc_by_category = defaultdict(int)

    for root, dirs, files in os.walk(PROJECT_ROOT):
        dirs[:] = [d for d in dirs if d not in EXCLUDE_DIRS]
        for file in files:
            file_path = os.path.join(root, file)
            if should_include_file(file_path):
                rel_path = os.path.relpath(file_path, PROJECT_ROOT)
                chunks, loc = process_file(file_path, rel_path)
                total_loc += loc
                cat = categorize_file(rel_path)
                loc_by_category[cat] += loc
                for out_name, text in chunks:
                    out_path = os.path.join(EXPORT_DIR, out_name)
                    with open(out_path, "w", encoding="utf-8") as out:
                        out.write(text)
                    all_files.append((rel_path, out_path))
                    print(f"Exported: {out_path} ({loc} LOC)")
    return all_files, total_loc, loc_by_category

def merge_files(all_files, total_loc, loc_by_category):
    os.makedirs(MERGED_DIR, exist_ok=True)
    buckets = {}
    index_lines = []
    size_report = {}

    for rel_path, file_path in all_files:
        cat = categorize_file(rel_path)
        if cat not in buckets:
            buckets[cat] = []
        buckets[cat].append((rel_path, file_path))

    for cat, files in buckets.items():
        current_text = ""
        part = 1
        files_in_part = []

        for rel_path, file_path in files:
            with open(file_path, "r", encoding="utf-8") as f:
                content = f.read()

            if len(current_text) + len(content) > MERGE_TARGET:
                out_path = os.path.join(MERGED_DIR, f"{cat}_part{part}.txt")
                with open(out_path, "w", encoding="utf-8") as out:
                    out.write(current_text)
                size_kb = os.path.getsize(out_path) // 1024
                size_report[out_path] = (size_kb, len(files_in_part))
                print(f"Merged: {out_path} ({size_kb} KB, {len(files_in_part)} files)")

                current_text = ""
                files_in_part = []
                part += 1

            current_text += "\n\n" + content
            index_lines.append(f"{rel_path}  -->  {cat}_part{part}.txt")
            files_in_part.append(rel_path)

        if current_text.strip():
            out_path = os.path.join(MERGED_DIR, f"{cat}_part{part}.txt")
            with open(out_path, "w", encoding="utf-8") as out:
                out.write(current_text)
            size_kb = os.path.getsize(out_path) // 1024
            size_report[out_path] = (size_kb, len(files_in_part))
            print(f"Merged: {out_path} ({size_kb} KB, {len(files_in_part)} files)")

    # Write INDEX.txt with size report + LOC
    index_path = os.path.join(MERGED_DIR, "INDEX.txt")
    with open(index_path, "w", encoding="utf-8") as idx:
        idx.write("=== NotebookLM Export Index ===\n")
        idx.write(f"Generated on: {datetime.now()}\n\n")

        idx.write("File → Merged Chunk Mapping\n\n")
        for line in index_lines:
            idx.write(line + "\n")

        idx.write("\n=== Chunk Size Report ===\n\n")
        for path, (size_kb, count) in size_report.items():
            idx.write(f"{os.path.basename(path)}  -->  {size_kb} KB, {count} files\n")

        idx.write("\n=== Project LOC Summary ===\n\n")
        for cat, loc in sorted(loc_by_category.items(), key=lambda x: -x[1]):
            idx.write(f"{cat}: {loc:,} LOC\n")
        idx.write(f"\nTOTAL: {total_loc:,} LOC\n")

    print(f"\n✅ Index created: {index_path}")
    print(f"✅ Export complete: {len(all_files)} files processed into {len(buckets)} categories.")
    print(f"✅ Total LOC: {total_loc:,}")
    for cat, loc in sorted(loc_by_category.items(), key=lambda x: -x[1]):
        print(f"   {cat}: {loc:,} LOC")

if __name__ == "__main__":
    files, total_loc, loc_by_category = export_project()
    merge_files(files, total_loc, loc_by_category)
    