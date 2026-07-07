# Codex Map Editor Setup Report

## Scope

- Task type: setup-only verification.
- Dependency install performed: user-local .NET 8 SDK install only.
- Map-editor implementation performed: no.
- `AegisMapDocument` added: no.
- Tiled import/export code added: no.
- Gameplay systems modified: no.

## Baseline

- Baseline source used: current repository contents in `E:\OpenRA Mod\ProjectAegisRTS`.
- Repository root: `E:\OpenRA Mod\ProjectAegisRTS`.
- Branch: `codex/terrain-sample-ground-tiles`.
- Commit: `1e0d7943628ffbe84d288309a2be0bc7717ffd1a`.
- Stage 1 used: no.
- Host OS: Microsoft Windows 11 Home 10.0.26200, 64-bit.
- Bash/apt setup script adaptation: ran Windows PowerShell equivalents because this environment is not Ubuntu/Debian.

## Required Files

- `AGENTS.md`: found at project root and reviewed.
- `CODEX_TILED_MAP_EDITOR_BRIEF.md`: missing. A recursive search under `E:\OpenRA Mod` found no matching file.
- `OpenRA-bleed.zip`: missing. An `E:\OpenRA Mod\OpenRA-bleed` folder exists, but it was not used.
- Stage 1 package files: not found and not used.
- `CnC_Red_Alert-main.zip`: not found. An `E:\OpenRA Mod\CnC_Red_Alert-main` folder exists, but no files from it were used.

## Project Checks

- `src/Rts.Core/Rts.Core.csproj`: found.
- `src/Rts.Core.Tests/Rts.Core.Tests.csproj`: found.
- Core target framework: `netstandard2.1`.
- Test target framework: `net8.0`.
- `src/Rts.Core` UnityEngine scan: no `UnityEngine`, `using UnityEngine`, or `Unity.` matches found.
- Determinism check: existing deterministic smoke tests passed in the baseline test run.

## Tool Versions

- Git: `git version 2.54.0.windows.1`.
- .NET SDK 8.x: installed and verified.
- .NET SDK used for baseline commands: `8.0.422` at `C:\Users\matth\.dotnet\dotnet.exe`.
- Default global `dotnet` on PATH: `10.0.301` at `C:\Program Files\dotnet\dotnet.exe`.
- User-local .NET SDKs installed: `8.0.422 [C:\Users\matth\.dotnet\sdk]`.
- User-local .NET runtimes relevant to tests: `Microsoft.NETCore.App 8.0.28`, `Microsoft.AspNetCore.App 8.0.28`, `Microsoft.WindowsDesktop.App 8.0.28`.
- Python 3: `Python 3.14.5`.
- npm: `11.14.1`.
- jq: missing.
- zip CLI: missing.
- unzip CLI: missing on PATH; Git-bundled `C:\Program Files\Git\usr\bin\unzip.exe` is present (`UnZip 6.00`).
- Tiled CLI: missing.
- OpenUPM CLI: missing.
- Unity Editor: missing from PATH.

## Build And Test Results

- `dotnet restore src/Rts.Core.Tests/Rts.Core.Tests.csproj`: passed using .NET SDK `8.0.422`.
- `dotnet build src/Rts.Core.Tests/Rts.Core.Tests.csproj --no-restore`: passed using .NET SDK `8.0.422`, with 0 warnings and 0 errors.
- `dotnet run --project src/Rts.Core.Tests/Rts.Core.Tests.csproj`: passed using .NET SDK `8.0.422`, 117/117 tests.

## Readiness Notes

- The setup did not fail merely because Unity Editor, Tiled GUI, SuperTiled2Unity, Tiled CLI, OpenUPM, or Unity are unavailable; those are editor-side validation tools.
- The missing `CODEX_TILED_MAP_EDITOR_BRIEF.md` is a required-file issue for the next implementation prompt.
- The next prompt should build the Tiled map-editor integration from Stage 0, not from Stage 1.
- No C&C / Red Alert implementation files were used.
- No protected Command & Conquer / Red Alert code, names, art, UI, map data, or faction identifiers were used.
- No OpenRA code was copied; OpenRA remains read-only reference unless GPL obligations are explicitly accepted and documented.
