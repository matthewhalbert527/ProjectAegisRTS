# Tools

Stage 0 has no external package requirement, but building/running the C# projects requires a .NET SDK.

- `run-stage0-tests.ps1`: runs `dotnet run --project src/Rts.Core.Tests`.
- `build-stage0-test-runner.ps1`: publishes `Rts.Core.Tests.exe` into `build/stage0-test-runner` and refreshes a desktop shortcut named `ProjectAegisRTS Stage 0 Tests.lnk`.

These scripts do not install software. If `dotnet` is missing, they fail with a clear message.
