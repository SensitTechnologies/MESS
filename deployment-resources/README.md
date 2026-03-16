# Blazor Publish Script

This script automates updating the repository and publishing the Blazor application using `dotnet publish`.

It ensures the repository is up-to-date with the remote `main` branch and then builds a clean **Release** publish output.

---

## What the Script Does

1. Determines the **repository root directory** using Git.
2. Verifies required project files exist.
3. Fetches the latest changes from the remote repository.
4. Fast-forwards the local `main` branch to match `origin/main`.
5. Cleans the existing `publish` directory.
6. Runs `dotnet publish` in **Release** configuration.
7. Outputs the compiled application into a `/publish` directory at the repo root.

---

## Requirements

The following tools must be installed:

- **Git**
- **.NET SDK** (compatible with the project)
- **Bash** (Linux/macOS or WSL on Windows)

---

## Project Structure Assumption

The script assumes the repository contains the Blazor project at:

```
MESS/MESS.Blazor/MESS.Blazor.csproj
```

The publish output will be placed in:

```
/publish
```

at the repository root.

Example:

```
repo-root/
│
├─ MESS/
│ └─ MESS.Blazor/
│ └─ MESS.Blazor.csproj
│
├─ publish/
│
└─ deployment-resources
  └─publish.sh
```
---

## Usage

### 1. Make the Script Executable

```bash
chmod +x publish.sh
```

### 2. Run the Script
```bash
./publish.sh
```
---
### Output

After running, the compiled application will be located in:

```
publish/
```
This directory contains the files produced by:
```
dotnet publish -c Release
```


These files can be copied directly to a server or deployment environment. They can also be ran directly from the machine
that runs the publish script.
---
### Safety Checks

The script will stop if:

* It is not run inside a Git repository

* The Blazor project file cannot be found

* The git pull cannot fast-forward

These checks help prevent publishing from an incorrect directory or outdated code.

---
### Configuration

At the top of the script you can modify:

```bash
BRANCH="main"
CONFIGURATION="Release"
```

### Example

Publish from a different branch:
```bash
BRANCH="develop"
```
**NOTE**: At this point in time, MESS branches are often created for a specific new feature in development. If you want 
to use a branch other than main (which is not recommended unless you really know what you are doing), check to see which
branches are currently available.

Publish using Debug configuration:
```bash
CONFIGURATION="Debug"
```

### Example Output
```
Fetching latest changes from origin...
Fast-forwarding 'main'...

Publishing from commit:
a1b2c3d Update production log service

Cleaning publish directory...

Publish completed successfully.
```
---
### Notes
* The script removes and recreates the /publish directory each run.
* Only fast-forward merges are allowed to prevent accidental overwrites.
* The script must be run from inside the repository.