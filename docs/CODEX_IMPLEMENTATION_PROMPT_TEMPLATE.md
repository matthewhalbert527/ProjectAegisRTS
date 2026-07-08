# Codex Implementation Prompt Template For A Completed Unit Packet

Use this prompt after `docs/UNIT_PACKET_TEMPLATE.md` has been filled out for one unit. Do not use this prompt with an incomplete packet.

```text
You are Codex running locally on my PC. Use extra-high reasoning.

PROJECT ROOT
E:\OpenRA Mod\ProjectAegisRTS

TASK
Implement exactly one new ProjectAegisRTS unit from the completed unit packet below.

COMPLETED UNIT PACKET
<paste the completed unit packet here, or provide the path to docs/units/<unit>_UNIT_PACKET.md>

BRANCH
Create and switch to:
codex/unit-<unit_type_id>

DO NOT
- Do not implement any unit that is not in the packet.
- Do not invent missing gameplay values. If a required value is missing, stop and report the missing field.
- Do not replace existing units unless the packet explicitly says to.
- Do not modify Rts.Core with UnityEngine references.
- Do not break PCDesktop right sidebar.
- Do not break QuestXR Stage4/Stage5 controls.
- Do not break Stage27.1 placement HUD separation.
- Do not weaken medium/full validation.
- Do not push.

PRESERVE
- Rts.Core deterministic gameplay authority.
- Existing Stage 0 through current stage behavior.
- Existing visual fallback prefabs for other actors.
- Existing production/sidebar behavior unless the packet explicitly adds this unit to a producer.

BASELINE
Run:

cd "E:\OpenRA Mod\ProjectAegisRTS"
dotnet run --no-restore --project src/Rts.Core.Tests
.\tools\build-rts-core-for-unity.ps1
.\tools\audit-medium-validation-recursion.ps1
if (Test-Path .\tools\audit-full-validation-recursion.ps1) { .\tools\audit-full-validation-recursion.ps1 }
.\tools\run-stage4-checks.ps1
.\tools\run-stage5-checks.ps1
git diff --check

Run the highest available current medium check. Prefer the newest run-stage*-medium-checks.ps1 by stage number.

IMPLEMENTATION STEPS

1. Inspect the completed packet.
   - Confirm every required field from docs/UNIT_PACKET_TEMPLATE.md is filled.
   - Confirm model, texture, icon, socket, movement, weapon, and validation sections are complete.
   - If the packet is incomplete, do not implement. Report missing fields.

2. Inspect existing examples.
   - Core unit examples in src/Rts.Core/Demo/DemoRules.cs.
   - Data contracts in src/Rts.Core/Data/Definitions.cs.
   - Existing visual definition in unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/light_tank_visual.asset.
   - Existing production prefab examples under unity/Assets/Rts/Art/Prefabs/Actors/.

3. Add core gameplay only as specified.
   - Add the new UnitDefinition in DemoRules.CreateDefaultRules().
   - Add or reuse WeaponDefinition exactly as specified.
   - Add production prerequisites and factory type exactly as specified.
   - Add the unit to the producing building's ProducesTypeIds only if the packet says it must be buildable.
   - Add focused tests in src/Rts.Core.Tests/Program.cs when the packet introduces new behavior or buildability.
   - Keep Rts.Core UnityEngine-free.

4. Import or create Unity assets.
   - Put source files under unity/Assets/Rts/Art/Source/Units/<unit_type_id>/.
   - Put textures under unity/Assets/Rts/Art/Textures/Units/<unit_type_id>/.
   - Put materials under unity/Assets/Rts/Art/Materials/Units/<unit_type_id>/.
   - Put the production prefab at unity/Assets/Rts/Art/Prefabs/Actors/Production/Units/<unit_type_id>/<unit_type_id>.prefab.
   - Put the icon at unity/Assets/Rts/Art/Icons/<unit_type_id>_icon.png.

5. Create the final prefab.
   - Use the packet's scale, pivot, orientation, and socket table.
   - Add ActorPrefabDescriptor.
   - Add ActorPrefabSocket components for every required socket.
   - Add LODGroup when practical.
   - Assign real materials and textures. Do not leave flat placeholder materials unless the packet explicitly calls for them as debug-only.
   - Ensure root origin is ground center and forward is +Z.
   - Verify the prefab is not just a cube, plane, or primitive fallback.

6. Create or update Unity data assets.
   - Create or update unity/Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/<unit_type_id>_visual.asset.
   - Set productionPrefab to the final prefab.
   - Keep generatedBlockoutPrefab/fallbackPrefab as fallback only.
   - Set preferred prefab mode to production.
   - Set motionProfileId from the packet.
   - Create or update VisualMotionProfile only if needed by the packet.
   - Create or update CombatVisualProfile only if needed by the packet.

7. Create a validation/review scene.
   - Create unity/Assets/Rts/Scenes/<unit_type_id>_UnitReview.unity.
   - Show the final prefab, a comparable existing unit for scale, and socket/weapon inspection helpers where practical.
   - Add movement/combat sandbox objects if the unit moves or fires.
   - Capture screenshot to build/screenshots/units/<unit_type_id>_review.png if practical.

8. Validate implementation.
   Run:

   dotnet run --no-restore --project src/Rts.Core.Tests
   .\tools\build-rts-core-for-unity.ps1
   .\tools\audit-medium-validation-recursion.ps1
   if (Test-Path .\tools\audit-full-validation-recursion.ps1) { .\tools\audit-full-validation-recursion.ps1 }
   .\tools\run-stage4-checks.ps1
   .\tools\run-stage5-checks.ps1

   Run the highest available current medium check again.

   Scan Rts.Core for UnityEngine references:

   if (Get-Command rg -ErrorAction SilentlyContinue) {
       rg "UnityEngine" .\src\Rts.Core
   } else {
       Get-ChildItem ".\src\Rts.Core" -Recurse -Include *.cs | Select-String -Pattern "UnityEngine"
   }

   Run:

   git diff --check

   If the packet says the unit must appear in the player-facing build, also run:

   .\tools\build-windows-player-stage16.ps1
   .\tools\inspect-latest-player-log.ps1

9. Commit locally only if all required validation passes.
   Commit message:
   Add <display_name> unit

FINAL REPORT
Report:
1. Branch
2. Commit hash, or explain why no commit was made
3. Unit type id and display name
4. Core files changed
5. Unity files changed
6. Prefab path
7. Visual definition path
8. Review scene path
9. Screenshots captured
10. Movement implementation result
11. Combat implementation result
12. Rts.Core test result
13. Stage4/Stage5 result
14. Current medium check result
15. Windows player build and Player.log result, if run
16. Rts.Core UnityEngine-free result
17. Working tree status
18. Exact command I should run next

Do not push.
```
