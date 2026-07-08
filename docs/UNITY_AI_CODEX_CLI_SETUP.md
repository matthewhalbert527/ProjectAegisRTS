# Unity AI and Codex CLI Setup

Last updated: 2026-07-05

## Current Local Setup

- Unity Editor: `E:\Unity\Hub\Editor\6000.5.1f1\Editor\Unity.exe`
- Unity project: `E:\OpenRA Mod\ProjectAegisRTS\unity`
- Unity AI Assistant package: `com.unity.ai.assistant` at `2.13.0-pre.2`
- Codex CLI: `codex-cli 0.139.0`
- Codex Unity MCP server name: `unity-mcp`
- Unity relay executable: `C:\Users\matth\.unity\relay\relay_win.exe`
- Unity Assistant MCP client config: `unity\UserSettings\mcp.json`

`unity\UserSettings\mcp.json` is intentionally ignored by git because it is a machine-local Unity settings file.

## Connection Directions

There are two related but different integrations:

1. Codex CLI as an MCP client talking to Unity.
   - Codex launches Unity's relay in MCP mode.
   - This lets Codex ask Unity for scene/project information and run Unity MCP tools after the Editor allows the connection.

2. Unity AI Assistant as an MCP client talking to Codex.
   - Unity Assistant can launch `codex mcp-server`.
   - This lets Assistant use Codex-provided MCP tools when Unity's Assistant MCP extensions page loads the local config.

## Codex CLI MCP Registration

Configured with:

```powershell
codex mcp add unity-mcp -- "C:\Users\matth\.unity\relay\relay_win.exe" --mcp --project-path "E:\OpenRA Mod\ProjectAegisRTS\unity"
```

Verify with:

```powershell
codex mcp get unity-mcp
codex mcp list
```

Expected command:

```text
C:\Users\matth\.unity\relay\relay_win.exe
```

Expected args:

```text
--mcp --project-path E:\OpenRA Mod\ProjectAegisRTS\unity
```

## Unity Assistant MCP Config

Local config file:

```text
E:\OpenRA Mod\ProjectAegisRTS\unity\UserSettings\mcp.json
```

It registers:

```json
{
  "enabled": true,
  "path": "C:\\Users\\matth\\AppData\\Roaming\\npm",
  "mcpServers": {
    "codex-cli": {
      "type": "stdio",
      "command": "C:\\Users\\matth\\AppData\\Roaming\\npm\\codex.cmd",
      "args": ["mcp-server"],
      "env": {}
    }
  }
}
```

## First Interactive Unity Step

Unity may still require a one-time interactive approval.

1. Open the project in Unity.
2. Open `Edit > Project Settings > AI > Unity MCP Server`.
3. Ensure the Unity MCP bridge is running.
4. Approve the first connection request from Codex if Unity prompts.
5. Open the Assistant MCP Extensions page and confirm `codex-cli` is listed.

## Smoke Test

With the Unity Editor open on the project, run:

```powershell
cd "E:\OpenRA Mod\ProjectAegisRTS"
codex "Use the Unity MCP tools to inspect the currently open scene and summarize the hierarchy."
```

If the relay cannot connect, check:

- Unity Editor is open to `E:\OpenRA Mod\ProjectAegisRTS\unity`.
- `C:\Users\matth\.unity\relay\relay_win.exe --help` works.
- Unity has accepted the MCP connection.
- Unity AI Assistant package is still present in `unity\Packages\manifest.json`.

## Source References

- Unity AI package docs installed locally:
  `unity\Library\PackageCache\com.unity.ai.assistant@6fee27370e6a\Documentation~`
- Unity AI package documentation URL:
  `https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.13/manual/index.html`
- Codex MCP documentation:
  `https://developers.openai.com/codex/mcp`
